﻿using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMQWonderwareAdapter
{

    class RMQManager
    {
        public class EventArgsMessageArrived : EventArgs
        {
            public BasicDeliverEventArgs Message { get; set; }
            public string OriginalMessageString { get; set; }
            public EventingBasicConsumer sender { get; set; }
        }

        public event EventHandler<EventArgsMessageArrived> MessageArrived;
        private void PostMessageArrived(EventingBasicConsumer sender, BasicDeliverEventArgs s, string MessageString)
        {
            if (MessageArrived != null)
            {
                MessageArrived(this, new EventArgsMessageArrived() { sender = sender, Message = s, OriginalMessageString = MessageString });
            }
        }


        public RMQManager()
        {
        }

        public string HostName, VirtualHost, UserName, Password, PC_ID;
        public int Port;

        // topic exchanges
        public string InboundExchangeName, InboundQueueName;  // this program accepts programmatic commands on an inbound queue: shutdown (closes program), update (redo database queries and GUI display)
        public string OutboundExchangeName; // this program sends information about its actions to an outbound queue: program opened, program closing

        IConnection conn = null;
        IModel channel = null;
        string consumerTag = null;

        public async Task<bool> Connect()
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.AutomaticRecoveryEnabled = true; // RabbitMQ .NET/C# client supports automatic recovery of connections and topology (queues, exchanges, bindings, and consumers).
            factory.RequestedHeartbeat = 1; // Keepalives

            factory.HostName = HostName;
            factory.Port = Port;
            factory.VirtualHost = VirtualHost;
            factory.UserName = UserName;
            factory.Password = Password;

            try
            {
                await Task.Run(() =>
                {
                    conn = factory.CreateConnection();
                    channel = conn.CreateModel();
                });

                if (channel != null)
                    CreateFabric();

                return true;
            }
            catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException ex)
            {
                Console.Write(ex.ToString());
                return false;
            }

        }

        public async Task<bool> Disconnect()
        {
            try
            {
                await Task.Run(() =>
                {
                    if (channel != null)
                        channel.Close();

                    if (conn != null)
                        conn.Close();
                });

                return true;
            }
            catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException ex)
            {
                Console.Write(ex.ToString());
                return false;
            }
        }

        delegate void Consumer_Received_(object sender, BasicDeliverEventArgs e);

        private void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var ch = sender as RabbitMQ.Client.Events.EventingBasicConsumer;

            var s = System.Text.Encoding.UTF8.GetString(e.Body);

            PostMessageArrived(ch, e, s);
        }

        public bool PutMessage(string key, string s)
        {
            // recommended: use PC_ID in the key
            byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(s);

            try
            {
                if (channel != null)
                {
                    channel.BasicPublish(OutboundExchangeName, key, null, messageBodyBytes);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }

            return false;
        }

        public async Task<bool> Unsubscribe()
        {
            if (consumerTag == null)
                return true;

            try
            {
                await Task.Run(() =>
                {
                    if (channel != null)
                        channel.BasicCancel(consumerTag);
                    consumerTag = null;
                });
                return true;
            }
            catch (AlreadyClosedException ex)
            {
                Console.Write(ex.ToString());
            }
            catch (System.TimeoutException tex)
            {
                Console.Write(tex.ToString());
            }

            return false;
        }

        private void CreateFabric()
        {
            // WARNING: if you change any of the definitions of queues or exchanges, you will get a BrokerUnreachableException or AlreadyClosedException at some point while the program is running
            // because the durable definitions on the server DON'T MATCH the definitions in this function or in the AutomaticRecoveryEnabled feature (which attempts to recreate them with their last C# definitions!)
            // the solution is to delete the definitions of queues and exchanges on the server using the RMQ Web Administration GUI.

            InboundQueueName = channel.QueueDeclare().QueueName;
            //channel.QueueDeclare( queue: InboundQueueName, durable: true, exclusive: false, autoDelete: false);

            channel.ExchangeDeclare(exchange: InboundExchangeName, type: "topic", durable: true, autoDelete: false);

            channel.QueueBind(queue: InboundQueueName,
                   exchange: InboundExchangeName,
                   routingKey: "#");

            // don't Subscribe() yet.

            channel.ExchangeDeclare(exchange: OutboundExchangeName, type: "topic", durable: true, autoDelete: false);

        }

        public bool Subscribe()
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += Consumer_Received;

            try
            {
                if (channel != null)
                    consumerTag = channel.BasicConsume(queue: InboundQueueName,
                                     autoAck: true,
                                     consumer: consumer);
                return consumerTag != null;
            }
            catch (RabbitMQ.Client.Exceptions.OperationInterruptedException rex)
            {
                Console.Write(rex.ToString());
                consumer.Received -= Consumer_Received;
                return false;
            }

        }


    }
}
