﻿using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using TuesPechkin.Util;

namespace TuesPechkin
{
    public class StandardAssembly : MarshalByRefObject, IAssembly
    {
        public bool Loaded { get; private set; }

        public string Path { get; private set; }

        public StandardAssembly()
        {
        }

        public StandardAssembly(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            Path = path;
        }

        public void Load(string pathOverride = null)
        {
            if (Loaded)
            {
                throw new AssemblyAlreadyLoadedException();
            }

            if (pathOverride != null)
            {
                Path = pathOverride;
            }

            WinApiHelper.LoadLibrary(Path);
            WkhtmltoxBindings.wkhtmltopdf_init(0);

            Loaded = true;
        }

        #region Rest of IAssembly stuff
        public IntPtr CreateGlobalSettings()
        {
            Tracer.Trace("T:" + Thread.CurrentThread.Name + " Creating global settings (wkhtmltopdf_create_global_settings)");

            return WkhtmltoxBindings.wkhtmltopdf_create_global_settings();
        }

        public IntPtr CreateObjectSettings()
        {
            Tracer.Trace("T:" + Thread.CurrentThread.Name + " Creating object settings (wkhtmltopdf_create_object_settings)");

            return WkhtmltoxBindings.wkhtmltopdf_create_object_settings();
        }

        public int SetGlobalSetting(IntPtr setting, string name, string value)
        {
            Tracer.Trace(
                String.Format(
                    "T:{0} Setting global setting '{1}' to '{2}' for config {3}",
                    Thread.CurrentThread.Name,
                    name,
                    value,
                    setting));

            var success = WkhtmltoxBindings.wkhtmltopdf_set_global_setting(setting, name, value);

            Tracer.Trace(String.Format("...setting was {0}", success == 1 ? "successful" : "not successful"));

            return success;
        }

        public unsafe string GetGlobalSetting(IntPtr setting, string name)
        {
            Tracer.Trace("T:" + Thread.CurrentThread.Name + " Getting global setting (wkhtmltopdf_get_global_setting)");

            byte[] buf = new byte[2048];

            fixed (byte* p = buf)
            {
                WkhtmltoxBindings.wkhtmltopdf_get_global_setting(setting, name, p, buf.Length);
            }

            int walk = 0;

            while (walk < buf.Length && buf[walk] != 0)
            {
                walk++;
            }

            return Encoding.UTF8.GetString(buf, 0, walk);
        }

        public int SetObjectSetting(IntPtr setting, string name, string value)
        {
            Tracer.Trace(
                String.Format(
                    "T:{0} Setting object setting '{1}' to '{2}' for config {3}",
                    Thread.CurrentThread.Name,
                    name,
                    value,
                    setting));

            var success = WkhtmltoxBindings.wkhtmltopdf_set_object_setting(setting, name, value);

            Tracer.Trace(String.Format("...setting was {0}", success == 1 ? "successful" : "not successful"));

            return success;
        }

        public unsafe string GetObjectSetting(IntPtr setting, string name)
        {
            Tracer.Trace(string.Format(
                "T:{0} Getting object setting '{1}' for config {2}",
                Thread.CurrentThread.Name,
                name,
                setting));

            byte[] buf = new byte[2048];

            fixed (byte* p = buf)
            {
                WkhtmltoxBindings.wkhtmltopdf_get_object_setting(setting, name, p, buf.Length);
            }

            int walk = 0;

            while (walk < buf.Length && buf[walk] != 0)
            {
                walk++;
            }

            return Encoding.UTF8.GetString(buf, 0, walk);
        }

        public IntPtr CreateConverter(IntPtr globalSettings)
        {
            Tracer.Trace("T:" + Thread.CurrentThread.Name + " Creating converter (wkhtmltopdf_create_converter)");

            return WkhtmltoxBindings.wkhtmltopdf_create_converter(globalSettings);
        }

        public void DestroyConverter(IntPtr converter)
        {
            Tracer.Trace("T:" + Thread.CurrentThread.Name + " Destroying converter (wkhtmltopdf_destroy_converter)");

            WkhtmltoxBindings.wkhtmltopdf_destroy_converter(converter);

            PinnedCallbacks.Unregister(converter);
        }

        public void SetWarningCallback(IntPtr converter, StringCallback callback)
        {
            Tracer.Trace("T:" + Thread.CurrentThread.Name + " Setting warning callback (wkhtmltopdf_set_warning_callback)");
            
            WkhtmltoxBindings.wkhtmltopdf_set_warning_callback(converter, callback);

            PinnedCallbacks.Register(converter, callback);
        }

        public void SetErrorCallback(IntPtr converter, StringCallback callback)
        {
            Tracer.Trace("T:" + Thread.CurrentThread.Name + " Setting error callback (wkhtmltopdf_set_error_callback)");
            
            WkhtmltoxBindings.wkhtmltopdf_set_error_callback(converter, callback);

            PinnedCallbacks.Register(converter, callback);
        }

        public void SetFinishedCallback(IntPtr converter, IntCallback callback)
        {
            Tracer.Trace("T:" + Thread.CurrentThread.Name + " Setting finished callback (wkhtmltopdf_set_finished_callback)");

            WkhtmltoxBindings.wkhtmltopdf_set_finished_callback(converter, callback);

            PinnedCallbacks.Register(converter, callback);
        }

        public void SetPhaseChangedCallback(IntPtr converter, VoidCallback callback)
        {
            Tracer.Trace("T:" + Thread.CurrentThread.Name + " Setting phase change callback (wkhtmltopdf_set_phase_changed_callback)");

            WkhtmltoxBindings.wkhtmltopdf_set_phase_changed_callback(converter, callback);

            PinnedCallbacks.Register(converter, callback);
        }

        public void SetProgressChangedCallback(IntPtr converter, IntCallback callback)
        {
            Tracer.Trace("T:" + Thread.CurrentThread.Name + " Setting progress change callback (wkhtmltopdf_set_progress_changed_callback)");

            WkhtmltoxBindings.wkhtmltopdf_set_progress_changed_callback(converter, callback);

            PinnedCallbacks.Register(converter, callback);
        }

        public bool PerformConversion(IntPtr converter)
        {
            Tracer.Trace("T:" + Thread.CurrentThread.Name + " Starting conversion (wkhtmltopdf_convert)");

            return WkhtmltoxBindings.wkhtmltopdf_convert(converter) != 0;
        }

        public void AddObject(IntPtr converter, IntPtr objectConfig, string html)
        {
            Tracer.Trace("T:" + Thread.CurrentThread.Name + " Adding string object (wkhtmltopdf_add_object)");

            WkhtmltoxBindings.wkhtmltopdf_add_object(converter, objectConfig, html);
        }

        public void AddObject(IntPtr converter, IntPtr objectConfig, byte[] html)
        {
            Tracer.Trace("T:" + Thread.CurrentThread.Name + " Adding byte[] object (wkhtmltopdf_add_object)");

            WkhtmltoxBindings.wkhtmltopdf_add_object(converter, objectConfig, html);
        }

        public int GetPhaseNumber(IntPtr converter)
        {
            Tracer.Trace("T:" + Thread.CurrentThread.Name + " Requesting current phase (wkhtmltopdf_current_phase)");

            return WkhtmltoxBindings.wkhtmltopdf_current_phase(converter);
        }

        public int GetPhaseCount(IntPtr converter)
        {
            Tracer.Trace("T:" + Thread.CurrentThread.Name + " Requesting phase count (wkhtmltopdf_phase_count)");

            return WkhtmltoxBindings.wkhtmltopdf_phase_count(converter);
        }

        public string GetPhaseDescription(IntPtr converter, int phase)
        {
            Tracer.Trace("T:" + Thread.CurrentThread.Name + " Requesting phase description (wkhtmltopdf_phase_description)");

            return Marshal.PtrToStringAnsi(WkhtmltoxBindings.wkhtmltopdf_phase_description(converter, phase));
        }

        public string GetProgressDescription(IntPtr converter)
        {
            Tracer.Trace("T:" + Thread.CurrentThread.Name + " Requesting progress string (wkhtmltopdf_progress_string)");

            return Marshal.PtrToStringAnsi(WkhtmltoxBindings.wkhtmltopdf_progress_string(converter));
        }

        public int GetHttpErrorCode(IntPtr converter)
        {
            Tracer.Trace("T:" + Thread.CurrentThread.Name + " Requesting http error code (wkhtmltopdf_http_error_code)");

            return WkhtmltoxBindings.wkhtmltopdf_http_error_code(converter);
        }

        public byte[] GetConverterResult(IntPtr converter)
        {
            Tracer.Trace("T:" + Thread.CurrentThread.Name + " Requesting converter result (wkhtmltopdf_get_output)");

            IntPtr tmp;
            var len = WkhtmltoxBindings.wkhtmltopdf_get_output(converter, out tmp);
            var output = new byte[len];
            Marshal.Copy(tmp, output, 0, output.Length);
            return output;
        }
        #endregion

        private static class PinnedCallbacks
        {
            private static readonly Dictionary<IntPtr, List<Delegate>> registry = new Dictionary<IntPtr,List<Delegate>>();

            public static void Register(IntPtr converter, Delegate callback)
            {
                List<Delegate> delegates;

                if (!registry.TryGetValue(converter, out delegates))
                {
                    delegates = new List<Delegate>();
                    registry.Add(converter, delegates);
                }

                delegates.Add(callback);
            }

            public static void Unregister(IntPtr converter)
            {
                registry.Remove(converter);
            }
        }
    }
}