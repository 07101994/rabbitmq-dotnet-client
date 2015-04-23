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

using NUnit.Framework;
using RabbitMQ.Client.Impl;
using System;
using System.Threading;

namespace RabbitMQ.Client.Unit
{
    [TestFixture]
    internal class TestHeartbeats : IntegrationFixture
    {
        private const UInt16 heartbeatTimeout = 2;

        [Test]
        [Category("Focus")]
        public void TestThatHeartbeatWriterUsesConfigurableInterval()
        {
            var cf = new ConnectionFactory() { RequestedHeartbeat = heartbeatTimeout, AutomaticRecoveryEnabled = false };
            var conn = cf.CreateConnection();
            var ch = conn.CreateModel();
            bool wasShutdown = false;

            conn.ConnectionShutdown += (sender, evt) =>
            {
                lock(conn)
                {
                    wasShutdown = true;
                }
            };
            Thread.Sleep(heartbeatTimeout * 10 * 1000);

            Assert.IsFalse(wasShutdown, "shutdown event should not have been fired");
            Assert.IsTrue(conn.IsOpen, "connection should be open");
        }
    }
}