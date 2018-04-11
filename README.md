# RMQWonderwareAdapter
Subscribes to PLC tags from Wonderware DA server and publish them into RabbitMQ

This C# project listens on one RabbitMQ channel for commands like subscribe to a PLC tag (any attribute of an object in the Galaxy) and then writes changes from that tag into another RabbitMQ channel. It can also write PLC tags based on a RabbitMQ message.

Since RabbitMQ has a STOMP adapter, this means that a mobile client or website can directly subscribe to these channels and receive PLC updates quickly.

You must download MXAccess Toolkit for System Platform 2014 from https://softwaresupportsp.schneider-electric.com to get the required DLL (ArchestrA.MXAccess.dll). There are also Wonderware licensing requirements (it must run on a platform in the Galaxy with at least an InTouch WindowViewer license).


