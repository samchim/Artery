//-----------------------------------------------------------------------
// <copyright file="SessionApi.cs" company="Google LLC">
//
// Copyright 2019 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace Google.XR.ARCoreExtensions.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
    using AndroidImport = Google.XR.ARCoreExtensions.Internal.DllImportNoop;
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using IOSImport = Google.XR.ARCoreExtensions.Internal.DllImportNoop;
#endif

    internal class SessionApi
    {
        public static void ReleaseFrame(IntPtr frameHandle)
        {
            ExternApi.ArFrame_release(frameHandle);
        }

        public static void UpdateSessionConfig(
            IntPtr sessionHandle,
            IntPtr configHandle,
            ARCoreExtensionsConfig config)
        {
#if UNITY_ANDROID
            ApiCloudAnchorMode cloudAnchorMode = (ApiCloudAnchorMode)config.CloudAnchorMode;
            ExternApi.ArConfig_setCloudAnchorMode(
                    sessionHandle, configHandle, cloudAnchorMode);
#endif
        }

        public static IntPtr HostCloudAnchor(
            IntPtr sessionHandle,
            IntPtr anchorHandle)
        {
            IntPtr cloudAnchorHandle = IntPtr.Zero;
            ApiArStatus status = ExternApi.ArSession_hostAndAcquireNewCloudAnchor(
                sessionHandle,
                anchorHandle,
                ref cloudAnchorHandle);
            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat("Failed to host a new Cloud Anchor, status '{0}'", status);
            }

            return cloudAnchorHandle;
        }

        public static IntPtr HostCloudAnchor(
            IntPtr sessionHandle,
            IntPtr anchorHandle,
            int ttlDays)
        {
            IntPtr cloudAnchorHandle = IntPtr.Zero;
            ApiArStatus status = ExternApi.ArSession_hostAndAcquireNewCloudAnchorWithTtl(
                sessionHandle, anchorHandle, ttlDays, ref cloudAnchorHandle);
            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat("Failed to host a Cloud Anchor with TTL {0}, status '{1}'",
                    ttlDays, status);
            }

            return cloudAnchorHandle;
        }

        public static void SetAuthToken(IntPtr sessionHandle, string authToken)
        {
            ExternApi.ArSession_setAuthToken(sessionHandle, authToken);
        }

        public static IntPtr ResolveCloudAnchor(
            IntPtr sessionHandle,
            string cloudAnchorId)
        {
            IntPtr cloudAnchorHandle = IntPtr.Zero;
            ApiArStatus status = ExternApi.ArSession_resolveAndAcquireNewCloudAnchor(
                sessionHandle,
                cloudAnchorId,
                ref cloudAnchorHandle);
            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat("Failed to resolve a new Cloud Anchor, status '{0}'", status);
            }

            return cloudAnchorHandle;
        }

        public static FeatureMapQuality EstimateFeatureMapQualityForHosting(
            IntPtr sessionHandle, Pose pose)
        {
            IntPtr poseHandle = PoseApi.Create(sessionHandle, pose);
            int featureMapQuality = (int)FeatureMapQuality.Insufficient;
            var status = ExternApi.ArSession_estimateFeatureMapQualityForHosting(
                sessionHandle, poseHandle, ref featureMapQuality);
            PoseApi.Destroy(poseHandle);
            if (status != ApiArStatus.Success)
            {
                Debug.LogErrorFormat("Failed to estimate feature map quality with status '{0}'.",
                    status);
            }

            return (FeatureMapQuality)featureMapQuality;
        }

        public static RecordingStatus GetRecordingStatus(IntPtr sessionHandle)
        {
            ApiRecordingStatus apiStatus = ApiRecordingStatus.None;
            ExternApi.ArSession_getRecordingStatus(sessionHandle, ref apiStatus);
            return Translators.ToRecordingStatus(apiStatus);
        }

        public static RecordingResult StartRecording(IntPtr sessionHandle,
                                                     ARCoreRecordingConfig config)
        {
            IntPtr recordingConfigHandle = RecordingConfigApi.Create(sessionHandle, config);

            ApiArStatus status = ExternApi.ArSession_startRecording(
                sessionHandle, recordingConfigHandle);

            RecordingConfigApi.Destroy(recordingConfigHandle);

            // Only specific ArStatus responses are expected.
            switch (status)
            {
                case ApiArStatus.Success:
                    return RecordingResult.OK;
                case ApiArStatus.ErrorIllegalState:
                    return RecordingResult.ErrorIllegalState;
                case ApiArStatus.ErrorInvalidArgument:
                    return RecordingResult.ErrorInvalidArgument;
                case ApiArStatus.ErrorRecordingFailed:
                    return RecordingResult.ErrorRecordingFailed;
                default:
                    Debug.LogErrorFormat("Attempt to start a recording failed with unexpected " +
                        "status: {0}", status);
                    break;
            }

            return RecordingResult.ErrorRecordingFailed;
        }

        public static RecordingResult StopRecording(IntPtr sessionHandle)
        {
            ApiArStatus status = ExternApi.ArSession_stopRecording(sessionHandle);

            // Only specific ArStatus responses are expected.
            switch (status)
            {
                case ApiArStatus.Success:
                    return RecordingResult.OK;
                case ApiArStatus.ErrorRecordingFailed:
                    return RecordingResult.ErrorRecordingFailed;
                default:
                    Debug.LogErrorFormat("Attempt to stop recording failed with unexpected " +
                        "status: {0}", status);
                    break;
            }

            return RecordingResult.ErrorRecordingFailed;
        }

        public static PlaybackStatus GetPlaybackStatus(IntPtr sessionHandle)
        {
            ApiPlaybackStatus apiStatus = ApiPlaybackStatus.None;
            ExternApi.ArSession_getPlaybackStatus(sessionHandle, ref apiStatus);
            return apiStatus.ToPlaybackStatus();
        }

        public static PlaybackResult SetPlaybackDataset(IntPtr sessionHandle,
                                                        string datasetFilepath)
        {
            ApiArStatus status =
                ExternApi.ArSession_setPlaybackDataset(sessionHandle, datasetFilepath);

            // Only specific ArStatus responses are expected.
            switch (status)
            {
                case ApiArStatus.Success:
                    return PlaybackResult.OK;
                case ApiArStatus.ErrorSessionNotPaused:
                    return PlaybackResult.ErrorSessionNotPaused;
                case ApiArStatus.ErrorSessionUnsupported:
                    return PlaybackResult.ErrorSessionUnsupported;
                case ApiArStatus.ErrorPlaybackFailed:
                    return PlaybackResult.ErrorPlaybackFailed;
                default:
                    Debug.LogErrorFormat("Attempt to playback recording failed with unexpected " +
                        "status: {0}", status);
                    break;
            }

            return PlaybackResult.ErrorPlaybackFailed;
        }

        [SuppressMessage("UnityRules.UnityStyleRules", "US1113:MethodsMustBeUpperCamelCase",
            Justification = "External call.")]
        private struct ExternApi
        {
            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArFrame_release(IntPtr frameHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_hostAndAcquireNewCloudAnchor(
                IntPtr sessionHandle,
                IntPtr anchorHandle,
                ref IntPtr cloudAnchorHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_resolveAndAcquireNewCloudAnchor(
                IntPtr sessionHandle,
                string cloudAnchorId,
                ref IntPtr cloudAnchorHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_hostAndAcquireNewCloudAnchorWithTtl(
                IntPtr sessionHandle,
                IntPtr anchorHandle,
                int ttlDays,
                ref IntPtr cloudAnchorHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_setAuthToken(
                IntPtr sessionHandle,
                String authToken);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_estimateFeatureMapQualityForHosting(
                IntPtr sessionHandle,
                IntPtr poseHandle,
                ref int featureMapQuality);

#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_getConfig(
                IntPtr sessionHandle,
                IntPtr configHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_configure(
                IntPtr sessionHandle,
                IntPtr configHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_create(
                IntPtr sessionHandle,
                ref IntPtr configHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_destroy(
                IntPtr configHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_setCloudAnchorMode(
                IntPtr sessionHandle,
                IntPtr configHandle,
                ApiCloudAnchorMode mode);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArConfig_getCloudAnchorMode(
                IntPtr sessionHandle,
                IntPtr configHandle,
                ref ApiCloudAnchorMode mode);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_getRecordingStatus(
                IntPtr sessionHandle, ref ApiRecordingStatus recordingStatus);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_startRecording(
                IntPtr sessionHandle, IntPtr recordingConfigHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_stopRecording(
                IntPtr sessionHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArSession_getPlaybackStatus(
                IntPtr sessionHandle, ref ApiPlaybackStatus playbackStatus);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern ApiArStatus ArSession_setPlaybackDataset(
                IntPtr sessionHandle, string datasetFilepath);
#pragma warning restore 626
        }
    }
}
