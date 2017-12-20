// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace System.Tests
{
    public class StringGetHashCodeTests : RemoteExecutorTestBase
    {
        //[Fact]
        public void GetHashCode_UseSameStringInTwoProcesses_ReturnsDifferentHashCodes()
        {
            EnsureHashCodeDifferentForChildString("abc".GetHashCode().ToString());
        }

        private static void EnsureHashCodeDifferentForChildString(string origHashCode, string timesInvoked = "0")
        {
            RemoteInvoke((parentHash, stimes) =>
            {
                int times;
                int.TryParse(stimes, out times);

                string childHash = "abc".GetHashCode().ToString();
                if (parentHash.Equals(childHash) && times < 3)
                {
                    // 1 in 4 billion chance the child and parent hashcode are the same. To further reduce change of collision we retry up to 3 times
                    EnsureHashCodeDifferentForChildString(parentHash, (times + 1).ToString());
                }
                else
                {
                    Assert.NotEqual(parentHash, childHash.ToString());
                }
                return SuccessExitCode;
            }, origHashCode.ToString(), (timesInvoked + 1).ToString()).Dispose();
        }

        [Fact]
        public void GetHashCode_UseDictionary_SetThreshold_UsesNonDefaultComparer()
        {
            IEqualityComparer<string> comparer = EqualityComparer<string>.Default;
            var dict = new Dictionary<string, string>(comparer);
            Console.WriteLine($"default comparer name = {dict.Comparer.GetType().ToString()}"); // System.Collections.Generic.NonRandomizedStringEqualityComparer
            
            Assembly mscorlib = typeof(string).Assembly;
            Type type = mscorlib.GetType("System.Collections.HashHelpers"); // HashHelpers.HashCollisionThreshold is const, and can't be changed at runtime

            // add enough entries to dictionary so the collision count goes past threshold (100). 
            string refPath = @"C:\CodeHub\corefx\reference.txt"; // @"/Users/mariyan/CodeHub/corefx/reference.txt";
            string[] readText = File.ReadAllLines(refPath);
            foreach (string s in readText)
            {
                if (dict.ContainsKey(s))
                {
                    Console.WriteLine($"{s} is already there");
                }
                else
                {
                    dict.Add(s, s);
                }
                Console.WriteLine($"comparer on dict with Count {dict.Count} is {dict.Comparer.GetType().ToString()}");
            }

            // then assert the comparer to be changed from default
        }

        private static int _exactCount;

        //[Fact]
        public void ProperTestForDictionaryGetHashCode()
        {
            string path = @"C:\CodeHub\corefx\concurrentlog.txt"; // @"/Users/mariyan/CodeHub/corefx/concurrentlog.txt";
            string otherPath = @"C:\CodeHub\corefx\concurrentlog_temp.txt"; // @"/Users/mariyan/CodeHub/corefx/concurrentlog_temp.txt";

            var lookup = new HashSet<string>();
            string refPath = @"C:\CodeHub\corefx\reference.txt"; // @"/Users/mariyan/CodeHub/corefx/reference.txt";
            string[] readText = File.ReadAllLines(refPath);
            var mainHash = readText[0].GetOldHashCode();
            foreach (string s in readText)
            {
                lookup.Add(s);
                Console.WriteLine($"GetHashCode is same? {s.GetOldHashCode() == mainHash} {lookup.Count} {readText.Length}");
            }

            File.WriteAllLines(path, new List<string>() { "Strings with same hashcode" });
            File.WriteAllLines(otherPath, new string[] { "Starting" });
            
            var sw = Stopwatch.StartNew();
            _exactCount = lookup.Count;
            int numCores = System.Environment.ProcessorCount;

            ParallelExtensions.While(
                () => { return _exactCount < 100; },
                numCores,
                () => { return new HashSet<string>(); },
                (str, loopControl, localD) =>
                {
                    Console.WriteLine("starting one");
                    int estimatedCount = _exactCount;
                    string strX;
                    var iterator = GetNextString().GetEnumerator();
                    while (iterator.MoveNext())
                    {
                        strX = iterator.Current;
                        if (strX.GetOldHashCode() == mainHash && !localD.Contains(strX))
                        {
                            localD.Add(strX);
                            estimatedCount++;
                            File.AppendAllLines(otherPath, new string[] { "estimatedCount " + estimatedCount + " strX " + strX });
                            break;
                        }
                    }
                    Console.WriteLine("stoping one");
                    return localD;
                },
                (localD) =>
                {
                    lock (lookup)
                    {
                        foreach (var item in localD)
                        {
                            if (!lookup.Contains(item))
                            {
                                lookup.Add(item);
                            }
                        }
                        _exactCount = lookup.Count;
                    }
                });
            sw.Stop();
            
            TimeSpan ts = sw.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);

            File.AppendAllLines(path, new string[] { "RunTime " + elapsedTime });
            File.AppendAllLines(path, lookup);
            File.AppendAllLines(refPath, lookup);
        }

        private IEnumerable<string> GetNextString()
        {
            while (true)
            {
                yield return Guid.NewGuid().ToString();
            }
        }
    }

    public static class ParallelExtensions
    {
        public static void While(
            Func<bool> stopCondition,
            int numCores,
            Func<HashSet<string>> localInit, 
            Func<string, ParallelLoopState, HashSet<string>, HashSet<string>> body, 
            Action<HashSet<string>> localFinally)
        {
            var numParallel = numCores < 5 ? numCores : numCores - 2;
            Parallel.ForEach(Infinite(stopCondition), new ParallelOptions() { MaxDegreeOfParallelism = numParallel}, localInit, (str, loopState, hashset) => { return body(str, loopState, hashset); }, localFinally);
        }

        private static IEnumerable<string> Infinite(Func<bool> condition)
        {
            while (condition())
                yield return Guid.NewGuid().ToString();
        }
    }

    public static class StringExtensions
    {
        public static int GetOldHashCode(this string str)
        {
            return str.GetLegacyNonRandomizedHashCode();
        }

        private static int GetLegacyNonRandomizedHashCode(this string str)
        {
            unsafe
            {
                fixed (char* src = str)
                {
                    int hash1 = 5381;
                    int hash2 = hash1;

                    int c;
                    char* s = src;
                    while ((c = s[0]) != 0)
                    {
                        hash1 = ((hash1 << 5) + hash1) ^ c;
                        c = s[1];
                        if (c == 0)
                            break;
                        hash2 = ((hash2 << 5) + hash2) ^ c;
                        s += 2;
                    }

                    return hash1 + (hash2 * 1566083941);
                }
            }
        }
        
        private static int GetLegacyNonRandomizedHashCode32Bit(this string str)
        {
            int length = str.Length;
            unsafe
            {
                fixed (char* src = str)
                {
                    int hash1 = (5381 << 16) + 5381;
                    int hash2 = hash1;

                    // 32 bit machines.
                    int* pint = (int*)src;
                    int len = length;
                    while (len > 2)
                    {
                        hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
                        hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ pint[1];
                        pint += 2;
                        len -= 4;
                    }

                    if (len > 0)
                    {
                        hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
                    }

                    return hash1 + (hash2 * 1566083941);
                }
            }
        }
    }
}
