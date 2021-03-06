﻿// Copyright 2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"). You may not 
// use this file except in compliance with the License. A copy of the License 
// is located at
// 
// 	http://aws.amazon.com/apache2.0/
// 
// or in the "LICENSE" file accompanying this file. This file is distributed 
// on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either 
// express or implied. See the License for the specific language governing 
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Security.Authentication;
using Amazon.Runtime;

namespace AwsLabs
{
    /// <summary>
    ///     Class for creating a cusomizable credentials provider chain.
    /// </summary>
    public class CustomCredentialsProviderChain
    {
        private delegate AWSCredentials CredentialsGenerator();

        private readonly Dictionary<string, CredentialsGenerator> _credentialsGenerators =
            new Dictionary<string, CredentialsGenerator>();

        /// <summary>
        ///     Create an instance of the class using this credentials provider chain:
        ///     InstanceProfileAWSCredentials
        ///     EnvironmentAWSCredentials
        ///     SystemEnvironmentAWSCredentials
        /// </summary>
        public CustomCredentialsProviderChain()
        {
            AddProvider<InstanceProfileAWSCredentials>();
            AddProvider<EnvironmentAWSCredentials>();
            AddProvider<SystemEnvironmentAWSCredentials>();
        }

        /// <summary>
        ///     Get the first set of valid credentials in the credentials provider chain defined in this class.
        /// </summary>
        /// <returns>AWS credentials</returns>
        public AWSCredentials GetCredentials()
        {
            var exceptions = new List<Exception>();

            foreach (var generator in _credentialsGenerators)
            {
                AWSCredentials credentials = null;
                try
                {
                    credentials = generator.Value();
                    _Default.LogMessageToPage("({0}) {1}", generator.Key, "Credentials found.");
                    return credentials;
                }
                catch (Exception ex)
                {
                    _Default.LogMessageToPage("({0}) {1}", generator.Key, ex.ToString());
                }
            }
            _Default.LogMessageToPage("No credentials found.");
            throw new AuthenticationException("No credentials found.");
        }

        /// <summary>
        ///     Add a new class to the end of the credentials provider chain.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the class to add. The class must be derived from the AWSCredentials class and support
        ///     the use of the new operator.
        /// </typeparam>
        public void AddProvider<T>() where T : AWSCredentials, new()
        {
            string providerName = typeof (T).Name;

            if (!_credentialsGenerators.ContainsKey(providerName))
            {
                _credentialsGenerators.Add(providerName, () => new T());
            }
            else
            {
                throw new Exception(String.Format("A provider of type {0} is already registered.", providerName));
            }
        }

        /// <summary>
        ///     Remove a class from the credentials provider chain.
        /// </summary>
        /// <typeparam name="T">The type of the class to remove.</typeparam>
        public void RemoveProvider<T>() where T : AWSCredentials, new()
        {
            string providerName = typeof (T).Name;

            if (_credentialsGenerators.ContainsKey(providerName))
            {
                _credentialsGenerators.Remove(providerName);
            }
        }

        /// <summary>
        ///     Remove all entries from the credentials provider chain.
        /// </summary>
        public void Clear()
        {
            _credentialsGenerators.Clear();
        }
    }
}
