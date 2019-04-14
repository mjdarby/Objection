using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PorcupineCS;

namespace PorcupineTest
{
    [TestClass]
    public class MainTest
    {
        private List<string> paths;
        private List<float> senses;

        private string GetAbsRootPath()
        {
            return $"{Environment.CurrentDirectory}/../../../../../../";
        }

        [TestInitialize]
        public void Init()
        {
            if (!File.Exists($"libpv_porcupine{GetExtension()}"))
                File.Copy($"{GetAbsRootPath()}lib/{GetEnvironmentName()}/amd64/libpv_porcupine.dll", $"./libpv_porcupine{GetExtension()}");

            if(!File.Exists("porcupine_params.pv"))
                File.Copy($"{GetAbsRootPath()}lib/common/porcupine_params.pv", "./porcupine_params.pv");

            if (!File.Exists("porcupine.wav"))
                File.Copy($"{GetAbsRootPath()}resources/audio_samples/porcupine.wav", "./porcupine.wav");

            if (!File.Exists("multiple_keywords.wav"))
                File.Copy($"{GetAbsRootPath()}resources/audio_samples/multiple_keywords.wav", "./multiple_keywords.wav");

            paths = new List<string>();
            List<string> temp = new List<string>()
            {
                "alexa", "americano", "avocado", "blueberry", "bumblebee", "caterpillar", "christina",
                "dragonfly", "flamingo", "francesca", "grapefruit", "grasshopper", "iguana", "picovoice",
                "pineapple", "porcupine", "raspberry", "terminator", "vancouver"
            };
            foreach (string name in temp)
            {
                paths.Add($"{GetAbsRootPath()}resources/keyword_files/{GetEnvironmentName()/{name}_{GetEnvironmentName()}.ppn".Replace("/", "\\"));
            }
            senses = new List<float>();
            for (int i= 0; i < paths.Count; i++)
            {
                senses.Add(0.5f);
            }

            //Console.WriteLine();
        }


        [TestMethod]
        public void MultipleKeywords()
        {
            Porcupine p = new Porcupine(Path.Combine(Environment.CurrentDirectory, "porcupine_params.pv"), keywordFilePaths: paths, sensitivities: senses);
            WAVFile file = new WAVFile();
            file.Open("multiple_keywords.wav", WAVFile.WAVFileMode.READ);
            Assert.AreEqual(p.SampleRate(), file.AudioFormat.SampleRateHz, "The samplerate is not equal!!!");
            List<short> data = new List<short>();
            while (file.NumSamplesRemaining > 0)
            {
                data.Add(BitConverter.ToInt16(file.GetNextSample_ByteArray()));
            }
            int framecount = (int)Math.Floor((decimal)(data.Count / p.FrameLength()));
            var results = new List<int>();
            for (int i = 0; i < framecount; i++)
            {
                int start = i * p.FrameLength();
                int count = p.FrameLength();
                List<short> frame = data.GetRange(start, count);
                PicoVoiceStatus status = p.ProcessMultipleKeywords(frame.ToArray(), out int result);
                if(result >= 0) 
                    results.Add(result);
                Assert.AreEqual(PicoVoiceStatus.SUCCESS, status, "The status is not as expected");
            }

            var requiredRes = new[] {15, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18};

            Assert.AreEqual(requiredRes.Length, results.Count, $"expected results length are different expected {requiredRes.Length} got {results.Count}");
            for (var i = 0; i < results.Count; i++)
            {
                Assert.AreEqual(requiredRes[i], results[i], $"The result is not as expected, expected {requiredRes[i]} got {results[i]}");
            }

            p.Dispose();
        }

        [TestMethod]
        public void TestProcess()
        {
            Porcupine p = new Porcupine(Path.Combine(Environment.CurrentDirectory, "porcupine_params.pv"), keywordFilePath:$"{GetAbsRootPath()}resources/keyword_files/porcupine_{GetEnvironmentName()}.ppn", sensitivity:0.5f);
            Assert.AreEqual(PicoVoiceStatus.SUCCESS, p.Status, "the status of the creation of the recognition system has failed");
            WAVFile file = new WAVFile();
            file.Open("porcupine.wav", WAVFile.WAVFileMode.READ);
            Assert.AreEqual(p.SampleRate() / 1000, file.BitsPerSample, "The samplerate is not equal!!!");
            List<short> data = new List<short>();
            while (file.NumSamplesRemaining > 0)
            {
                data.Add(BitConverter.ToInt16(file.GetNextSample_ByteArray()));
            }

            int framecount = (int) Math.Floor((decimal) (data.Count / p.FrameLength()));
            var results = new List<bool>();
            for (int i = 0; i < framecount;  i++)
            {
                int start = i * p.FrameLength();
                int count = p.FrameLength();
                List<short> frame = data.GetRange(start, count);
                p.Process(frame.ToArray(), out bool result);
                results.Add(result);
            }

            var res = results.Count(x => x);

            Assert.AreEqual(1, res, $"The result is not as expected, expected {1} got {res}");
            p.Dispose();
        }

        #region utility

        private static string GetExtension()
        {
            PlatformID platform = Environment.OSVersion.Platform;
            if (platform == PlatformID.MacOSX)
            {
                return ".dylib";
            }

            if (platform == PlatformID.Unix)
            {
                return ".so";
            }

            if (platform == PlatformID.Win32NT)
            {
                return ".dll";
            }

            throw new NotImplementedException("this OS has no binding logic implemented yet.");
        }

        private static string GetEnvironmentName()
        {
            PlatformID platform = Environment.OSVersion.Platform;
            if (platform == PlatformID.MacOSX)
            {
                return "mac";
            }

            if (platform == PlatformID.Unix)
            {
                return "linux";
            }

            if (platform == PlatformID.Win32NT)
            {
                return "windows";
            }

            throw new NotImplementedException("this OS has no binding logic implemented yet.");
        }

        #endregion
    }
}
