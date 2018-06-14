﻿/* Copyright 2018-present MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;

namespace MongoDB.Driver.Core.Operations
{
    internal static class RetryabilityHelper
    {
        // private static fields
        private static readonly HashSet<ServerErrorCode> __notResumableChangeStreamErrorCodes;
        private static readonly HashSet<Type> __resumableChangeStreamExceptions;
        private static readonly HashSet<Type> __retryableWriteExceptions;
        private static readonly HashSet<ServerErrorCode> __retryableWriteErrorCodes;

        // static constructor
        static RetryabilityHelper()
        {
            var resumableAndRetryableExceptions = new HashSet<Type>()
            {
                typeof(MongoConnectionException),
                typeof(MongoNotPrimaryException),
                typeof(MongoNodeIsRecoveringException)
            };

            __resumableChangeStreamExceptions = new HashSet<Type>(resumableAndRetryableExceptions)
            {
                typeof(MongoCursorNotFoundException)
            };

            __retryableWriteExceptions = new HashSet<Type>(resumableAndRetryableExceptions)
            {
            };

            var resumableAndRetryableErrorCodes = new HashSet<ServerErrorCode>
            {
                ServerErrorCode.HostNotFound,
                ServerErrorCode.HostUnreachable,
                ServerErrorCode.NetworkTimeout,
                ServerErrorCode.SocketException
            };

            __retryableWriteErrorCodes = new HashSet<ServerErrorCode>(resumableAndRetryableErrorCodes)
            {
                ServerErrorCode.WriteConcernFailed
            };

            __notResumableChangeStreamErrorCodes = new HashSet<ServerErrorCode>()
            {
                ServerErrorCode.CappedPositionLost,
                ServerErrorCode.CursorKilled,
                ServerErrorCode.Interrupted
            };
        }

        // public static methods
        public static bool IsResumableChangeStreamException(Exception exception)
        {
            var commandException = exception as MongoCommandException;
            if (commandException != null)
            {
                var code = (ServerErrorCode)commandException.Code;
                return !__notResumableChangeStreamErrorCodes.Contains(code);
            }
            else
            {
                return __resumableChangeStreamExceptions.Contains(exception.GetType());
            }
        }

        public static bool IsRetryableWriteException(Exception exception)
        {
            if (__retryableWriteExceptions.Contains(exception.GetType()))
            {
                return true;
            }

            var commandException = exception as MongoCommandException;
            if (commandException != null)
            {
                var code = (ServerErrorCode)commandException.Code;
                return __retryableWriteErrorCodes.Contains(code);
            }

            return false;
        }
    }
}
