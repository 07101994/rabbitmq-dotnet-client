// This source code is dual-licensed under the Apache License, version
// 2.0, and the Mozilla Public License, version 1.1.
//
// The APL v2.0:
//
//---------------------------------------------------------------------------
//   Copyright (C) 2007-2013 GoPivotal, Inc.
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

using NUnit.Framework;

using System;
using System.Collections.Generic;

using RabbitMQ.Client.Exceptions;

namespace RabbitMQ.Client.Unit {
    [TestFixture]
    public class TestQueueDeclare : IntegrationFixture {

        [Test]
        public void TestDoubleQueueDeclareWithEquivalentArgs()
        {
            string q = GenerateQueueName();
            Model.QueueDeclare(q, false, false, false, null);
            VerifyEquivalent(Model, q, false, false, false, null);

            WithTemporaryModel((m) => m.QueueDelete(q));
        }

        [Test]
        public void TestDoubleQueueDeclareWithNonEquivalentArgs()
        {
            string q = GenerateQueueName();
            Model.QueueDeclare(q, false, false, false, null);
            VerifyNonEquivalent(Model, q, true, true, true, null);

            WithTemporaryModel((m) => m.QueueDelete(q));
        }

        protected void VerifyEquivalent(IModel m, string name, bool durable,
                                        bool exclusive, bool autoDelete, 
                                        IDictionary<string, object> args)
        {
            m.QueueDeclarePassive(name);
            m.QueueDeclare(name, durable, exclusive, autoDelete, args);
        }

        protected void VerifyNonEquivalent(IModel m, string name, bool durable,
                                           bool exclusive, bool autoDelete, 
                                           IDictionary<string, object> args)
        {
            m.QueueDeclarePassive(name);
            try
            {
                m.QueueDeclare(name, durable, exclusive, autoDelete, args);
                Assert.Fail("Expected queue.declare to throw");
            } catch (OperationInterruptedException eoi)
            {
                AssertPreconditionFailed(eoi.ShutdownReason);
            }
        }
    }
}
