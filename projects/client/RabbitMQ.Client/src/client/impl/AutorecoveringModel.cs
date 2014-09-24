// This source code is dual-licensed under the Apache License, version
// 2.0, and the Mozilla Public License, version 1.1.
//
// The APL v2.0:
//
//---------------------------------------------------------------------------
//   Copyright (C) 2007-2014 GoPivotal, Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//---------------------------------------------------------------------------
//
// The MPL v1.1:
//
//---------------------------------------------------------------------------
//  The contents of this file are subject to the Mozilla Public License
//  Version 1.1 (the "License"); you may not use this file except in
//  compliance with the License. You may obtain a copy of the License
//  at http://www.mozilla.org/MPL/
//
//  Software distributed under the License is distributed on an "AS IS"
//  basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See
//  the License for the specific language governing rights and
//  limitations under the License.
//
//  The Original Code is RabbitMQ.
//
//  The Initial Developer of the Original Code is GoPivotal, Inc.
//  Copyright (c) 2007-2014 GoPivotal, Inc.  All rights reserved.
//---------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing.Impl;

namespace RabbitMQ.Client.Impl
{
    public class AutorecoveringModel : IFullModel, IRecoverable
    {
        protected AutorecoveringConnection m_connection;
        protected Model m_delegate;

        protected List<ModelShutdownEventHandler> m_recordedShutdownEventHandlers =
            new List<ModelShutdownEventHandler>();

        public AutorecoveringModel(AutorecoveringConnection conn, Model _delegate)
        {
            this.m_connection = conn;
            this.m_delegate   = _delegate;
        }

        public void AutomaticallyRecover(AutorecoveringConnection conn, IConnection connDelegate)
        {
            this.m_connection = conn;
            this.m_delegate   = (Model)connDelegate.CreateModel();
            // TODO: inherit ack offset

            this.RecoverModelShutdownHandlers();
        }



        public event ModelShutdownEventHandler ModelShutdown
        {
            add
            {
                m_recordedShutdownEventHandlers.Add(value);
                m_delegate.ModelShutdown += value;
            }
            remove
            {
                m_recordedShutdownEventHandlers.Remove(value);
                m_delegate.ModelShutdown -= value;
            }
        }

        public event BasicReturnEventHandler BasicReturn
        {
            add
            {
                // TODO: record and re-add handlers
                m_delegate.BasicReturn += value;
            }
            remove
            {
                m_delegate.BasicReturn -= value;
            }
        }

        public event BasicAckEventHandler BasicAcks
        {
            add
            {
                // TODO: record and re-add handlers
                m_delegate.BasicAcks += value;
            }
            remove
            {
                m_delegate.BasicAcks -= value;
            }
        }

        public event BasicNackEventHandler BasicNacks
        {
            add
            {
                // TODO: record and re-add handlers
                m_delegate.BasicNacks += value;
            }
            remove
            {
                m_delegate.BasicNacks -= value;
            }
        }

        public event CallbackExceptionEventHandler CallbackException
        {
            add
            {
                // TODO: record and re-add handlers
                m_delegate.CallbackException += value;
            }
            remove
            {
                m_delegate.CallbackException -= value;
            }
        }

        public event FlowControlEventHandler FlowControl
        {
            add
            {
                // TODO: record and re-add handlers
                m_delegate.FlowControl += value;
            }
            remove
            {
                m_delegate.FlowControl -= value;
            }
        }

        public event BasicRecoverOkEventHandler BasicRecoverOk
        {
            add
            {
                // TODO: record and re-add handlers
                m_delegate.BasicRecoverOk += value;
            }
            remove
            {
                m_delegate.BasicRecoverOk -= value;
            }
        }

        public event RecoveryEventHandler Recovery
        {
            add
            {
                // TODO: record and re-add handlers
                m_delegate.Recovery += value;
            }
            remove
            {
                m_delegate.Recovery -= value;
            }
        }

        public int ChannelNumber
        {
            get
            {
                return m_delegate.ChannelNumber;
            }
        }

        public IBasicConsumer DefaultConsumer
        {
            get
            {
                return m_delegate.DefaultConsumer;
            }
            set
            {
                m_delegate.DefaultConsumer = value;
            }
        }




        void IDisposable.Dispose()
        {
            m_delegate.Close();
        }

        public void Close()
        {
            try
            {
                m_delegate.Close();
            } finally
            {
                m_connection.UnregisterModel(this);
            }
            
        }

        public void Close(ushort replyCode, string replyText)
        {
            try
            {
                m_delegate.Close(replyCode, replyText);
            } finally
            {
                m_connection.UnregisterModel(this);
            }
        }

        public void Abort()
        {
            try {
                m_delegate.Abort();
            } finally
            {
                m_connection.UnregisterModel(this);
            }
        }

        public void Abort(ushort replyCode, string replyText)
        {
            try
            {
                m_delegate.Abort(replyCode, replyText);
            }
            finally
            {
                m_connection.UnregisterModel(this);
            }
        }

        public void Close(ushort replyCode, string replyText, bool abort)
        {
            try
            {
                m_delegate.Close(replyCode, replyText, abort);
            }
            finally
            {
                m_connection.UnregisterModel(this);
            }
        }

        public void Close(ShutdownEventArgs reason, bool abort)
        {
            try
            {
                m_delegate.Close(reason, abort);
            }
            finally
            {
                m_connection.UnregisterModel(this);
            }
        }

        public void HandleChannelCloseOk()
        {
            m_delegate.HandleChannelCloseOk();
        }

        public ShutdownEventArgs CloseReason
        {
            get
            {
                return m_delegate.CloseReason;
            }
        }

        public bool IsOpen
        {
            get
            {
                return m_delegate.IsOpen;
            }
        }

        public bool IsClosed
        {
            get
            {
                return m_delegate.IsClosed;
            }
        }

        public ulong NextPublishSeqNo
        {
            get
            {
                return m_delegate.NextPublishSeqNo;
            }
        }

        public bool DispatchAsynchronous(Command cmd)
        {
            return m_delegate.DispatchAsynchronous(cmd);
        }

        public void HandleBasicDeliver(string consumerTag,
                                       ulong deliveryTag,
                                       bool redelivered,
                                       string exchange,
                                       string routingKey,
                                       IBasicProperties basicProperties,
                                       byte[] body)
        {
            m_delegate.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange,
                                          routingKey, basicProperties, body);
        }

        public void HandleBasicCancel(string consumerTag, bool nowait)
        {
            m_delegate.HandleBasicCancel(consumerTag, nowait);
        }

        public void HandleBasicReturn(ushort replyCode,
                                      string replyText,
                                      string exchange,
                                      string routingKey,
                                      IBasicProperties basicProperties,
                                      byte[] body)
        {
            m_delegate.HandleBasicReturn(replyCode, replyText, exchange,
                                         routingKey, basicProperties, body);
        }

        public void HandleBasicAck(ulong deliveryTag,
                                   bool multiple)
        {
            m_delegate.HandleBasicAck(deliveryTag, multiple);
        }

        public void HandleBasicNack(ulong deliveryTag,
                                    bool multiple,
                                    bool requeue)
        {
            m_delegate.HandleBasicNack(deliveryTag, multiple, requeue);
        }

        public void HandleChannelFlow(bool active)
        {
            m_delegate.HandleChannelFlow(active);
        }

        public void HandleConnectionStart(byte versionMajor,
                                          byte versionMinor,
                                          IDictionary<string, object> serverProperties,
                                          byte[] mechanisms,
                                          byte[] locales)
        {
            m_delegate.HandleConnectionStart(versionMajor, versionMinor, serverProperties,
                                             mechanisms, locales);
        }

        public void HandleConnectionClose(ushort replyCode,
                                          string replyText,
                                          ushort classId,
                                          ushort methodId)
        {
            m_delegate.HandleConnectionClose(replyCode, replyText, classId, methodId);
        }

        public void HandleConnectionBlocked(string reason)
        {
            m_delegate.HandleConnectionBlocked(reason);
        }

        public void HandleConnectionUnblocked()
        {
            m_delegate.HandleConnectionUnblocked();
        }

        public void HandleChannelClose(ushort replyCode,
                                       string replyText,
                                       ushort classId,
                                       ushort methodId)
        {
            m_delegate.HandleChannelClose(replyCode, replyText, classId, methodId);
        }

        public void FinishClose()
        {
            m_delegate.FinishClose();
        }

        public IBasicProperties CreateBasicProperties()
        {
            return m_delegate.CreateBasicProperties();
        }

        public void ExchangeDeclare(string exchange, string type, bool durable)
        {
            m_delegate.ExchangeDeclare(exchange, type, durable);
        }

        public void ExchangeDeclare(string exchange, string type)
        {
            m_delegate.ExchangeDeclare(exchange, type);
        }

        public void ExchangeDeclare(string exchange, string type, bool durable,
                                    bool autoDelete, IDictionary<string, object> arguments)
        {
            m_delegate.ExchangeDeclare(exchange, type, durable,
                                       autoDelete, arguments);
        }

        public void ExchangeDeclarePassive(string exchange)
        {
            m_delegate.ExchangeDeclarePassive(exchange);
        }

        public void ExchangeDeclareNoWait(string exchange,
                                          string type,
                                          bool durable,
                                          bool autoDelete,
                                          IDictionary<string, object> arguments)
        {
            m_delegate.ExchangeDeclareNoWait(exchange, type, durable, autoDelete, arguments);
        }

        public void ExchangeDelete(string exchange,
                                   bool ifUnused)
        {
            m_delegate.ExchangeDelete(exchange, ifUnused);
        }

        public void ExchangeDelete(string exchange)
        {
            m_delegate.ExchangeDelete(exchange);
        }

        public void ExchangeDeleteNoWait(string exchange,
                                         bool ifUnused)
        {
            m_delegate.ExchangeDeleteNoWait(exchange, ifUnused);
        }

        public void ExchangeBind(string destination,
                                 string source,
                                 string routingKey)
        {
            m_delegate.ExchangeBind(destination, source, routingKey);
        }

        public void ExchangeBind(string destination,
                                 string source,
                                 string routingKey,
                                 IDictionary<string, object> arguments)
        {
            m_delegate.ExchangeBind(destination, source, routingKey, arguments);
        }

        public void ExchangeBindNoWait(string destination,
                                       string source,
                                       string routingKey,
                                       IDictionary<string, object> arguments)
        {
            m_delegate.ExchangeBindNoWait(destination, source, routingKey, arguments);
        }

        public void ExchangeUnbind(string destination,
                                   string source,
                                   string routingKey,
                                   IDictionary<string, object> arguments)
        {
            m_delegate.ExchangeUnbind(destination, source, routingKey, arguments);
        }

        public void ExchangeUnbind(string destination,
                                   string source,
                                   string routingKey)
        {
            m_delegate.ExchangeUnbind(destination, source, routingKey);
        }

        public void ExchangeUnbindNoWait(string destination,
                                         string source,
                                         string routingKey,
                                         IDictionary<string, object> arguments)
        {
            m_delegate.ExchangeUnbind(destination, source, routingKey, arguments);
        }

        public QueueDeclareOk QueueDeclare()
        {
            return m_delegate.QueueDeclare();
        }

        public QueueDeclareOk QueueDeclarePassive(string queue)
        {
            return m_delegate.QueueDeclarePassive(queue);
        }

        public void QueueDeclareNoWait(string queue, bool durable, bool exclusive,
                                       bool autoDelete, IDictionary<string, object> arguments)
        {
            m_delegate.QueueDeclareNoWait(queue, durable, exclusive,
                                          autoDelete, arguments);
        }

        public QueueDeclareOk QueueDeclare(string queue, bool durable, bool exclusive,
                                           bool autoDelete, IDictionary<string, object> arguments)
        {
            return m_delegate.QueueDeclare(queue, durable, exclusive,
                                           autoDelete, arguments);
        }


        public void QueueBind(string queue,
                              string exchange,
                              string routingKey,
                              IDictionary<string, object> arguments)
        {
            m_delegate.QueueBind(queue, exchange, routingKey, arguments);
        }

        public void QueueBind(string queue,
                              string exchange,
                              string routingKey)
        {
            m_delegate.QueueBind(queue, exchange, routingKey);
        }

        public void QueueBindNoWait(string queue,
                                    string exchange,
                                    string routingKey,
                                    IDictionary<string, object> arguments)
        {
            m_delegate.QueueBind(queue, exchange, routingKey, arguments);
        }

        public void QueueUnbind(string queue,
                                string exchange,
                                string routingKey,
                                IDictionary<string, object> arguments)
        {
            m_delegate.QueueUnbind(queue, exchange, routingKey, arguments);
        }

        public uint QueuePurge(string queue)
        {
            return m_delegate.QueuePurge(queue);
        }

        public uint QueueDelete(string queue,
                                bool ifUnused,
                                bool ifEmpty)
        {
            return m_delegate.QueueDelete(queue, ifUnused, ifEmpty);
        }

        public uint QueueDelete(string queue)
        {
            return m_delegate.QueueDelete(queue);
        }

        public void QueueDeleteNoWait(string queue,
                                      bool ifUnused,
                                      bool ifEmpty)
        {
            m_delegate.QueueDelete(queue, ifUnused, ifEmpty);
        }

        public void ConfirmSelect()
        {
            m_delegate.ConfirmSelect();
        }

        public bool WaitForConfirms(TimeSpan timeout, out bool timedOut)
        {
            return m_delegate.WaitForConfirms(timeout, out timedOut);
        }

        public bool WaitForConfirms()
        {
            return m_delegate.WaitForConfirms();
        }

        public void WaitForConfirmsOrDie()
        {
            m_delegate.WaitForConfirmsOrDie();
        }

        public void WaitForConfirmsOrDie(TimeSpan timeout)
        {
            m_delegate.WaitForConfirmsOrDie(timeout);
        }

        public string BasicConsume(string queue,
                                   bool noAck,
                                   IBasicConsumer consumer)
        {
            return m_delegate.BasicConsume(queue, noAck, consumer);
        }

        public string BasicConsume(string queue,
                                   bool noAck,
                                   string consumerTag,
                                   IBasicConsumer consumer)
        {
            return m_delegate.BasicConsume(queue, noAck, consumerTag, consumer);
        }

        public string BasicConsume(string queue,
                                   bool noAck,
                                   string consumerTag,
                                   IDictionary<string, object> arguments,
                                   IBasicConsumer consumer)
        {
            return m_delegate.BasicConsume(queue, noAck, consumerTag, arguments, consumer);
        }

        public string BasicConsume(string queue,
                                   bool noAck,
                                   string consumerTag,
                                   bool noLocal,
                                   bool exclusive,
                                   IDictionary<string, object> arguments,
                                   IBasicConsumer consumer)
        {
            return m_delegate.BasicConsume(queue, noAck, consumerTag, noLocal,
                                           exclusive, arguments, consumer);
        }

        public void HandleBasicConsumeOk(string consumerTag)
        {
            m_delegate.HandleBasicConsumeOk(consumerTag);
        }

        public void BasicCancel(string consumerTag)
        {
            m_delegate.BasicCancel(consumerTag);
        }

        public void HandleBasicCancelOk(string consumerTag)
        {
            m_delegate.HandleBasicCancelOk(consumerTag);
        }

        public BasicGetResult BasicGet(string queue,
                                       bool noAck)
        {
            return m_delegate.BasicGet(queue, noAck);
        }

        public void BasicRecover(bool requeue)
        {
            m_delegate.BasicRecover(requeue);
        }

        public void BasicQos(uint prefetchSize,
                             ushort prefetchCount,
                             bool global)
        {
            m_delegate.BasicQos(prefetchSize, prefetchCount, global);
        }

        public void BasicPublish(PublicationAddress addr,
                                 IBasicProperties basicProperties,
                                 byte[] body)
        {
            m_delegate.BasicPublish(addr.ExchangeName,
                                    addr.RoutingKey,
                                    basicProperties,
                                    body);
        }

        public void BasicPublish(string exchange,
                                 string routingKey,
                                 IBasicProperties basicProperties,
                                 byte[] body)
        {
            m_delegate.BasicPublish(exchange,
                                    routingKey,
                                    basicProperties,
                                    body);
        }

        public void BasicPublish(string exchange,
                                 string routingKey,
                                 bool mandatory,
                                 IBasicProperties basicProperties,
                                 byte[] body)
        {
            m_delegate.BasicPublish(exchange,
                                    routingKey,
                                    mandatory,
                                    basicProperties,
                                    body);
        }

        public void BasicPublish(string exchange,
                                 string routingKey,
                                 bool mandatory,
                                 bool immediate,
                                 IBasicProperties basicProperties,
                                 byte[] body)
        {
            m_delegate.BasicPublish(exchange, routingKey, mandatory, immediate,
                                    basicProperties, body);
        }


        public void BasicAck(ulong deliveryTag,
                             bool multiple)
        {
            m_delegate.BasicAck(deliveryTag, multiple);
        }

        public void BasicReject(ulong deliveryTag,
                                bool requeue)
        {
            m_delegate.BasicReject(deliveryTag, requeue);
        }

        public void BasicNack(ulong deliveryTag,
                              bool multiple,
                              bool requeue)
        {
            m_delegate.BasicNack(deliveryTag, multiple, requeue);
        }

        public void BasicRecoverAsync(bool requeue)
        {
            m_delegate.BasicRecoverAsync(requeue);
        }

        public void TxSelect()
        {
            m_delegate.TxSelect();
        }

        public void TxCommit()
        {
            m_delegate.TxCommit();
        }

        public void TxRollback()
        {
            m_delegate.TxRollback();
        }




        public void HandleBasicGetOk(ulong deliveryTag,
                                     bool redelivered,
                                     string exchange,
                                     string routingKey,
                                     uint messageCount,
                                     IBasicProperties basicProperties,
                                     byte[] body){
            m_delegate.HandleBasicGetOk(deliveryTag, redelivered, exchange, routingKey,
                                        messageCount, basicProperties, body);
        }

        public void HandleBasicGetEmpty()
        {
            m_delegate.HandleBasicGetEmpty();
        }

        public void HandleBasicRecoverOk()
        {
            m_delegate.HandleBasicRecoverOk();
        }

        public void HandleCommand(ISession session, Command cmd)
        {
            m_delegate.HandleCommand(session, cmd);
        }

        public void OnSessionShutdown(ISession session, ShutdownEventArgs reason)
        {
            m_delegate.OnSessionShutdown(session, reason);
        }

        public bool SetCloseReason(ShutdownEventArgs reason)
        {
            return m_delegate.SetCloseReason(reason);
        }

        public virtual void OnModelShutdown(ShutdownEventArgs reason)
        {
            m_delegate.OnModelShutdown(reason);
        }

        public virtual void OnBasicReturn(BasicReturnEventArgs args)
        {
            m_delegate.OnBasicReturn(args);
        }

        public virtual void OnBasicAck(BasicAckEventArgs args)
        {
            m_delegate.OnBasicAck(args);
        }

        public virtual void OnBasicNack(BasicNackEventArgs args)
        {
            m_delegate.OnBasicNack(args);
        }

        public virtual void OnCallbackException(CallbackExceptionEventArgs args)
        {
            m_delegate.OnCallbackException(args);
        }

        public virtual void OnFlowControl(FlowControlEventArgs args)
        {
            m_delegate.OnFlowControl(args);
        }

        public virtual void OnBasicRecoverOk(EventArgs args)
        {
            m_delegate.OnBasicRecoverOk(args);
        }

        public void HandleQueueDeclareOk(string queue,
                                         uint messageCount,
                                         uint consumerCount)
        {
            m_delegate.HandleQueueDeclareOk(queue, messageCount, consumerCount);
        }

        public override string ToString() {
            return m_delegate.ToString();
        }


        protected void RecoverModelShutdownHandlers()
        {
            foreach(var eh in this.m_recordedShutdownEventHandlers)
            {
                this.m_delegate.ModelShutdown += eh;
            }
        }


        public void _Private_ExchangeDeclare(string exchange,
                                             string type,
                                             bool passive,
                                             bool durable,
                                             bool autoDelete,
                                             bool @internal,
                                             bool nowait,
                                             IDictionary<string, object> arguments)
        {
            _Private_ExchangeDeclare(exchange, type, passive,
                                     durable, autoDelete, @internal,
                                     nowait, arguments);
        }

        public void _Private_ExchangeDelete(string exchange,
                                            bool ifUnused,
                                            bool nowait)
        {
            _Private_ExchangeDelete(exchange, ifUnused, nowait);
        }

        public void _Private_ExchangeBind(string destination,
                                          string source,
                                          string routingKey,
                                          bool nowait,
                                          IDictionary<string, object> arguments)
        {
            _Private_ExchangeBind(destination, source, routingKey,
                                  nowait, arguments);
        }

        public void _Private_ExchangeUnbind(string destination,
                                            string source,
                                            string routingKey,
                                            bool nowait,
                                            IDictionary<string, object> arguments)
        {
            m_delegate._Private_ExchangeUnbind(destination, source, routingKey,
                                               nowait, arguments);
        }

        public void _Private_QueueDeclare(string queue,
                                          bool passive,
                                          bool durable,
                                          bool exclusive,
                                          bool autoDelete,
                                          bool nowait,
                                          IDictionary<string, object> arguments)
        {
            m_delegate._Private_QueueDeclare(queue, passive,
                                             durable, exclusive, autoDelete,
                                             nowait, arguments);
        }

        public void _Private_QueueBind(string queue,
                                       string exchange,
                                       string routingKey,
                                       bool nowait,
                                       IDictionary<string, object> arguments)
        {
            _Private_QueueBind(queue, exchange, routingKey,
                               nowait, arguments);
        }

        public uint _Private_QueuePurge(string queue,
                                        bool nowait)
        {
            return m_delegate._Private_QueuePurge(queue, nowait);
        }


        public uint _Private_QueueDelete(string queue,
                                         bool ifUnused,
                                         bool ifEmpty,
                                         bool nowait)
        {
            return m_delegate._Private_QueueDelete(queue, ifUnused,
                                                   ifEmpty, nowait);
        }

        public void _Private_BasicPublish(string exchange,
                                          string routingKey,
                                          bool mandatory,
                                          bool immediate,
                                          IBasicProperties basicProperties,
                                          byte[] body)
        {
            m_delegate._Private_BasicPublish(exchange, routingKey, mandatory,
                                             immediate, basicProperties, body);
        }

        public void _Private_BasicConsume(string queue,
                                          string consumerTag,
                                          bool noLocal,
                                          bool noAck,
                                          bool exclusive,
                                          bool nowait,
                                          IDictionary<string, object> arguments)
        {
            m_delegate._Private_BasicConsume(queue,
                                             consumerTag,
                                             noLocal,
                                             noAck,
                                             exclusive,
                                             nowait,
                                             arguments);
        }

        public void _Private_ConfirmSelect(bool nowait)
        {
            m_delegate._Private_ConfirmSelect(nowait);
        }

        public void _Private_BasicCancel(string consumerTag,
                                         bool nowait)
        {
            m_delegate._Private_BasicCancel(consumerTag,
                                            nowait);
        }

        public void _Private_ChannelOpen(string outOfBand)
        {
            m_delegate._Private_ChannelOpen(outOfBand);
        }

        public void _Private_ChannelCloseOk()
        {
            m_delegate._Private_ChannelCloseOk();
        }

        public void _Private_ChannelClose(ushort replyCode,
                                          string replyText,
                                          ushort classId,
                                          ushort methodId)
        {
            m_delegate._Private_ChannelClose(replyCode, replyText,
                                             classId, methodId);
        }

        public void _Private_BasicGet(string queue, bool noAck)
        {
            m_delegate._Private_BasicGet(queue, noAck);
        }

        public void _Private_BasicRecover(bool requeue)
        {
            m_delegate._Private_BasicRecover(requeue);
        }

        public void _Private_ChannelFlowOk(bool active)
        {
            m_delegate._Private_ChannelFlowOk(active);
        }

        public void _Private_ConnectionStartOk(IDictionary<string, object> clientProperties,
                                               string mechanism, byte[] response, string locale)
        {
            m_delegate._Private_ConnectionStartOk(clientProperties, mechanism,
                                                  response, locale);
        }

        public void HandleConnectionSecure(byte[] challenge)
        {
            m_delegate.HandleConnectionSecure(challenge);
        }

        public void _Private_ConnectionSecureOk(byte[] response)
        {
            m_delegate._Private_ConnectionSecureOk(response);
        }

        public void HandleConnectionTune(ushort channelMax,
                                         uint frameMax,
                                         ushort heartbeat)
        {
            m_delegate.HandleConnectionTune(channelMax, frameMax, heartbeat);
        }

        public void ConnectionTuneOk(ushort channelMax,
                                     uint frameMax,
                                     ushort heartbeat)
        {
            m_delegate.ConnectionTuneOk(channelMax, frameMax, heartbeat);
        }

        public void _Private_ConnectionOpen(string virtualHost,
                                            string capabilities,
                                            bool insist)
        {
            m_delegate._Private_ConnectionOpen(virtualHost, capabilities, insist);
        }

        public void HandleConnectionOpenOk(string knownHosts)
        {
            m_delegate.HandleConnectionOpenOk(knownHosts);
        }

        public void _Private_ConnectionClose(ushort replyCode,
                                             string replyText,
                                             ushort classId,
                                             ushort methodId)
        {
            m_delegate._Private_ConnectionClose(replyCode, replyText,
                                                classId, methodId);
        }

        public void _Private_ConnectionCloseOk()
        {
            m_delegate._Private_ConnectionCloseOk();
        }
    }
}