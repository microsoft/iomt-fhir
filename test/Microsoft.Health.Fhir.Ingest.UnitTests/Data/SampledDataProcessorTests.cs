// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class SampledDataProcessorTests
    {
        [Fact]
        public void GivenDataGoingPastEndBoundary_WhenBuildSampledData_ThenExcessDiscarded_Test()
        {
            var seedDateTime = new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var values = Enumerable.Range(0, 11)
                .Select(i => (seedDateTime.AddMinutes(i), i.ToString()))
                .ToArray();

            var result = SampledDataProcessor.Instance.BuildSampledData(values, seedDateTime, seedDateTime.AddMinutes(10).AddMilliseconds(-1), (decimal)TimeSpan.FromMinutes(1).TotalMilliseconds);
            Assert.NotNull(result);
            Assert.Equal("0 1 2 3 4 5 6 7 8 9", result);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        public void GivenDataStartingOnBoundaryAndEndingOnBoundary_WhenBuildSampledData_ThenSampledDataPopulated_Test(int totalSamples)
        {
            var period = TimeSpan.FromSeconds(1);

            // Normalize endBoundary to seconds being the most significant value
            var endBoundary = NormalizeToSecond(DateTime.UtcNow);
            var startBoundary = endBoundary.AddSeconds(-1 * (totalSamples - 1));
            var values = Enumerable.Range(1, totalSamples)
                .Select(i => (startBoundary.AddSeconds(i - 1), i.ToString()))
                .ToArray();

            Assert.Equal(endBoundary, values.Last().Item1);

            var result = SampledDataProcessor.Instance.BuildSampledData(values, startBoundary, endBoundary, (decimal)period.TotalMilliseconds);
            Assert.NotNull(result);

            var resultValues = result.Split(" ");
            Assert.Equal(values.Length, resultValues.Length);

            for (int i = 0; i < totalSamples; i++)
            {
                Assert.Equal(values[i].Item2, resultValues[i]);
            }
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        public void GivenDataStartingOnBoundaryAndEndingBeforeBoundary_WhenBuildSampledData_ThenSampledDataPopulatedWithERemaining_Test(int totalSamples)
        {
            var period = TimeSpan.FromSeconds(1);

            // Normalize endBoundary to seconds being the most significant value
            var endBoundary = NormalizeToSecond(DateTime.UtcNow);
            var startBoundary = endBoundary.AddSeconds(-1 * totalSamples);
            var values = Enumerable.Range(1, totalSamples)
                .Select(i => (startBoundary.AddSeconds(i - 1), i.ToString()))
                .ToArray();

            var result = SampledDataProcessor.Instance.BuildSampledData(values, startBoundary, endBoundary, (decimal)period.TotalMilliseconds);
            Assert.NotNull(result);

            var resultValues = result.Split(" ");
            Assert.Equal(totalSamples + 1, resultValues.Length);

            for (int i = 0; i < resultValues.Length; i++)
            {
                if (i < values.Length)
                {
                    Assert.Equal(values[i].Item2, resultValues[i]);
                }
                else
                {
                    Assert.Equal("E", resultValues[i]);
                }
            }
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        public void GivenDataStartingOnBoundaryAndMissingValues_WhenBuildSampledData_ThenSampledDataPopulatedWithEForMissingValues_Test(int totalSamples)
        {
            var period = TimeSpan.FromSeconds(1);

            // Normalize endBoundary to seconds being the most significant value
            var endBoundary = NormalizeToSecond(DateTime.UtcNow);
            var startBoundary = endBoundary.AddSeconds(-1 * (totalSamples - 1));
            var values = Enumerable.Range(1, totalSamples / 2)
                .Select(i => (startBoundary.AddSeconds((i - 1) * 2), i.ToString()))
                .ToArray();

            var result = SampledDataProcessor.Instance.BuildSampledData(values, startBoundary, endBoundary, (decimal)period.TotalMilliseconds);
            Assert.NotNull(result);

            var resultValues = result.Split(" ");
            Assert.Equal(values.Length * 2, resultValues.Length);

            for (int i = 0; i < totalSamples; i++)
            {
                if (i % 2 == 0)
                {
                    Assert.Equal(values[i / 2].Item2, resultValues[i]);
                }
                else
                {
                    Assert.Equal("E", resultValues[i]);
                }
            }
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        public void GivenDataStartingOffBoundary_WhenBuildSampledData_ThenSampledDataPopulated_Test(int totalSamples)
        {
            var period = TimeSpan.FromSeconds(1);

            // Normalize endBoundary to seconds being the most significant value
            var endBoundary = NormalizeToSecond(DateTime.UtcNow);
            var startBoundary = endBoundary.AddSeconds(-1 * (totalSamples - 1));
            var values = Enumerable.Range(1, totalSamples)
                .Select(i => (startBoundary.AddSeconds(i - 1).AddMilliseconds(5), i.ToString()))
                .ToArray();

            var result = SampledDataProcessor.Instance.BuildSampledData(values, startBoundary, endBoundary, (decimal)period.TotalMilliseconds);
            Assert.NotNull(result);

            var resultValues = result.Split(" ");
            Assert.Equal(values.Length, resultValues.Length);

            for (int i = 0; i < totalSamples; i++)
            {
                Assert.Equal(values[i].Item2, resultValues[i]);
            }
        }

        [Fact]
        public void GivenDataCollisions_WhenBuildSampledData_ThenLastValueUsed_Test()
        {
            var period = TimeSpan.FromSeconds(1);

            // Normalize endBoundary to seconds being the most significant value
            var endBoundary = NormalizeToSecond(DateTime.UtcNow);
            var startBoundary = endBoundary.AddSeconds(-1 * 9);
            var values = Enumerable.Range(1, 20)
                .Select(i => (startBoundary.AddMilliseconds((i * 500) - 499), i.ToString()))
                .ToArray();

            var result = SampledDataProcessor.Instance.BuildSampledData(values, startBoundary, endBoundary, (decimal)period.TotalMilliseconds);
            Assert.NotNull(result);

            var resultValues = result.Split(" ");
            Assert.Equal(10, resultValues.Length);

            for (int i = 0; i < resultValues.Length; i++)
            {
                Assert.Equal(values[((i + 1) * 2) - 1].Item2, resultValues[i]);
            }
        }

        [Fact]
        public void GivenDataStartingOffBoundary_WhenBuildSampledDataAndSampledDataToTimeValues_ThenNormalizedTimeValuesReturned_Test()
        {
            var period = TimeSpan.FromSeconds(1);

            // Normalize endBoundary to seconds being the most significant value
            var endBoundary = NormalizeToSecond(DateTime.UtcNow);
            var startBoundary = endBoundary.AddSeconds(-1 * 10);
            var values = Enumerable.Range(1, 10)
                .Select(i => (startBoundary.AddSeconds(i - 1).AddMilliseconds(5), i.ToString()))
                .ToArray();

            var sd = SampledDataProcessor.Instance.BuildSampledData(values, startBoundary, endBoundary, (decimal)period.TotalMilliseconds);
            var result = SampledDataProcessor.Instance.SampledDataToTimeValues(sd, startBoundary, (decimal)period.TotalMilliseconds);
            Assert.NotNull(result);

            Assert.Equal(values.Length, result.Length);

            for (int i = 0; i < result.Length; i++)
            {
                Assert.Equal(values[i].Item2, result[i].Value);
                Assert.Equal(NormalizeToSecond(values[i].Item1), result[i].Time);
            }
        }

        [Fact]
        public void GivenData1LongerThanData2AndData1StartsAndEnds_WhenMergeData_ThenExpectedValues_Test()
        {
            var seedDt = new DateTime(2019, 1, 1, 0, 30, 0, DateTimeKind.Utc);
            var data1 = new[]
            {
                seedDt,
                seedDt.AddMinutes(1),
                seedDt.AddMinutes(4),
            }
            .Select(d => (d, "1"))
            .ToArray();
            var data2 = new[]
            {
                seedDt.AddMinutes(2),
                seedDt.AddMinutes(3),
            }
            .Select(d => (d, "2"))
            .ToArray();

            var result = SampledDataProcessor.Instance.MergeData(data1, data2);

            Assert.Collection(
                result,
                v =>
                {
                    Assert.Equal(seedDt, v.Time);
                    Assert.Equal("1", v.Value);
                },
                v =>
                {
                    Assert.Equal(seedDt.AddMinutes(1), v.Time);
                    Assert.Equal("1", v.Value);
                },
                v =>
                {
                    Assert.Equal(seedDt.AddMinutes(2), v.Time);
                    Assert.Equal("2", v.Value);
                },
                v =>
                {
                    Assert.Equal(seedDt.AddMinutes(3), v.Time);
                    Assert.Equal("2", v.Value);
                },
                v =>
                {
                    Assert.Equal(seedDt.AddMinutes(4), v.Time);
                    Assert.Equal("1", v.Value);
                });
        }

        [Fact]
        public void GivenData1LongerThanData2AndData1StartsAndData2Ends_WhenMergeData_ThenExpectedValues_Test()
        {
            var seedDt = new DateTime(2019, 1, 1, 0, 30, 0, DateTimeKind.Utc);
            var data1 = new[]
            {
                seedDt,
                seedDt.AddMinutes(1),
                seedDt.AddMinutes(3),
            }
            .Select(d => (d, "1"))
            .ToArray();
            var data2 = new[]
            {
                seedDt.AddMinutes(2),
                seedDt.AddMinutes(4),
            }
            .Select(d => (d, "2"))
            .ToArray();

            var result = SampledDataProcessor.Instance.MergeData(data1, data2);

            Assert.Collection(
                result,
                v =>
                {
                    Assert.Equal(seedDt, v.Time);
                    Assert.Equal("1", v.Value);
                },
                v =>
                {
                    Assert.Equal(seedDt.AddMinutes(1), v.Time);
                    Assert.Equal("1", v.Value);
                },
                v =>
                {
                    Assert.Equal(seedDt.AddMinutes(2), v.Time);
                    Assert.Equal("2", v.Value);
                },
                v =>
                {
                    Assert.Equal(seedDt.AddMinutes(3), v.Time);
                    Assert.Equal("1", v.Value);
                },
                v =>
                {
                    Assert.Equal(seedDt.AddMinutes(4), v.Time);
                    Assert.Equal("2", v.Value);
                });
        }

        [Fact]
        public void GivenData2LongerThanData1AndData2StartsAndEnds_WhenMergeData_ThenExpectedValues_Test()
        {
            var seedDt = new DateTime(2019, 1, 1, 0, 30, 0, DateTimeKind.Utc);
            var data1 = new[]
            {
                seedDt.AddMinutes(2),
                seedDt.AddMinutes(3),
            }
            .Select(d => (d, "1"))
            .ToArray();
            var data2 = new[]
            {
                seedDt,
                seedDt.AddMinutes(1),
                seedDt.AddMinutes(4),
            }
            .Select(d => (d, "2"))
            .ToArray();

            var result = SampledDataProcessor.Instance.MergeData(data1, data2);

            Assert.Collection(
                result,
                v =>
                {
                    Assert.Equal(seedDt, v.Time);
                    Assert.Equal("2", v.Value);
                },
                v =>
                {
                    Assert.Equal(seedDt.AddMinutes(1), v.Time);
                    Assert.Equal("2", v.Value);
                },
                v =>
                {
                    Assert.Equal(seedDt.AddMinutes(2), v.Time);
                    Assert.Equal("1", v.Value);
                },
                v =>
                {
                    Assert.Equal(seedDt.AddMinutes(3), v.Time);
                    Assert.Equal("1", v.Value);
                },
                v =>
                {
                    Assert.Equal(seedDt.AddMinutes(4), v.Time);
                    Assert.Equal("2", v.Value);
                });
        }

        [Fact]
        public void GivenData2LongerThanData1AndData2StartsAndData1Ends_WhenMergeData_ThenExpectedValues_Test()
        {
            var seedDt = new DateTime(2019, 1, 1, 0, 30, 0, DateTimeKind.Utc);
            var data1 = new[]
            {
                seedDt.AddMinutes(2),
                seedDt.AddMinutes(4),
            }
            .Select(d => (d, "1"))
            .ToArray();
            var data2 = new[]
            {
                seedDt,
                seedDt.AddMinutes(1),
                seedDt.AddMinutes(3),
            }
            .Select(d => (d, "2"))
            .ToArray();

            var result = SampledDataProcessor.Instance.MergeData(data1, data2);

            Assert.Collection(
                result,
                v =>
                {
                    Assert.Equal(seedDt, v.Time);
                    Assert.Equal("2", v.Value);
                },
                v =>
                {
                    Assert.Equal(seedDt.AddMinutes(1), v.Time);
                    Assert.Equal("2", v.Value);
                },
                v =>
                {
                    Assert.Equal(seedDt.AddMinutes(2), v.Time);
                    Assert.Equal("1", v.Value);
                },
                v =>
                {
                    Assert.Equal(seedDt.AddMinutes(3), v.Time);
                    Assert.Equal("2", v.Value);
                },
                v =>
                {
                    Assert.Equal(seedDt.AddMinutes(4), v.Time);
                    Assert.Equal("1", v.Value);
                });
        }

        [Fact]
        public void GivenCollisions_WhenMergeData_ThenData1ValueUsed_Test()
        {
            var seedDt = new DateTime(2019, 1, 1, 0, 30, 0, DateTimeKind.Utc);
            var data1 = new[]
            {
                seedDt,
                seedDt.AddMinutes(1),
            }
            .Select(d => (d, "1"))
            .ToArray();
            var data2 = new[]
            {
                seedDt,
                seedDt.AddMinutes(1),
            }
            .Select(d => (d, "2"))
            .ToArray();

            var result = SampledDataProcessor.Instance.MergeData(data1, data2);

            Assert.Collection(
                result,
                v =>
                {
                    Assert.Equal(seedDt, v.Time);
                    Assert.Equal("1", v.Value);
                },
                v =>
                {
                    Assert.Equal(seedDt.AddMinutes(1), v.Time);
                    Assert.Equal("1", v.Value);
                });
        }

        private static DateTime NormalizeToSecond(DateTime value)
        {
            return value.Date
                .AddHours(value.Hour)
                .AddMinutes(value.Minute)
                .AddSeconds(value.Second);
        }
    }
}
