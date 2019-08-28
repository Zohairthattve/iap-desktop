﻿//
// Copyright 2019 Google LLC
//
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
//

using Google.Apis.Auth.OAuth2;
using Google.Solutions.Compute.Iap;
using Google.Solutions.Compute.Net;
using Google.Solutions.Compute.Test.Env;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Google.Solutions.Compute.Test.Iap
{
    [TestFixture]
    [Category("IntegrationTest")]
    public class TestSshRelayProber : FixtureBase
    {
        [Test]
        public void ProbingNonexistingProjectCausesUnauthorizedException()
        {
            var prober = new SshRelayProber(
                new IapTunnelingEndpoint(
                    GoogleCredential.GetApplicationDefault(),
                    new VmInstanceReference(
                        "invalid",
                        Defaults.Zone,
                        "invalid"),
                    80,
                    IapTunnelingEndpoint.DefaultNetworkInterface));

            AssertEx.ThrowsAggregateException<UnauthorizedException>(() =>
                prober.ProbeConnectionAsync(TimeSpan.FromSeconds(10)).Wait());
        }

        [Test]
        public void ProbingNonexistingZoneCausesUnauthorizedException()
        {
            var prober = new SshRelayProber(
                new IapTunnelingEndpoint(
                    GoogleCredential.GetApplicationDefault(),
                    new VmInstanceReference(
                        Defaults.ProjectId,
                        "invalid",
                        "invalid"),
                    80,
                    IapTunnelingEndpoint.DefaultNetworkInterface));

            AssertEx.ThrowsAggregateException<UnauthorizedException>(() =>
                prober.ProbeConnectionAsync(TimeSpan.FromSeconds(10)).Wait());
        }

        [Test]
        public void ProbingNonexistingInstanceCausesUnauthorizedException()
        {
            var prober = new SshRelayProber(
                new IapTunnelingEndpoint(
                    GoogleCredential.GetApplicationDefault(),
                    new VmInstanceReference(
                        Defaults.ProjectId,
                        Defaults.Zone,
                        "invalid"),
                    80,
                    IapTunnelingEndpoint.DefaultNetworkInterface));

            AssertEx.ThrowsAggregateException<UnauthorizedException>(() =>
                prober.ProbeConnectionAsync(TimeSpan.FromSeconds(10)).Wait());
        }

        [Test]
        public async Task ProbeExistingInstanceOverRdpSucceeds(
             [WindowsInstance] InstanceRequest testInstance)
        {
            await testInstance.AwaitReady();

            var prober = new SshRelayProber(
                new IapTunnelingEndpoint(
                    GoogleCredential.GetApplicationDefault(),
                    testInstance.InstanceReference,
                    3389,
                    IapTunnelingEndpoint.DefaultNetworkInterface));

            await prober.ProbeConnectionAsync(TimeSpan.FromSeconds(10));
        }

        [Test]
        public async Task ProbeExistingInstanceOverWrongCausesNetworkStreamClosedException(
             [WindowsInstance] InstanceRequest testInstance)
        {
            await testInstance.AwaitReady();

            var prober = new SshRelayProber(
                new IapTunnelingEndpoint(
                    GoogleCredential.GetApplicationDefault(),
                    testInstance.InstanceReference,
                    22,
                    IapTunnelingEndpoint.DefaultNetworkInterface));

            AssertEx.ThrowsAggregateException<NetworkStreamClosedException>(() =>
                prober.ProbeConnectionAsync(TimeSpan.FromSeconds(5)).Wait());
        }
    }
}
