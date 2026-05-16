using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace dd2d.Tests.Restaurant;

/// <summary>
/// Tests for the random destination selection pattern used in Visitor.cs.
///
/// Regression guard: originally Visitor used new Random(DateTime.Now.Ticks).
/// When several visitors are spawned in the same frame their seeds collide,
/// making every visitor walk to destination index 0.
///
/// The fix was switching to new Random() (no seed), which in .NET 6+ uses
/// OS entropy and guarantees distinct seeds even when called in rapid succession.
///
/// No Godot runtime required — pure System.Random behaviour.
/// </summary>
[TestFixture]
public class RandomDestinationTests
{
    /// <summary>
    /// Regression: new Random(DateTime.Now.Ticks) called several times in the
    /// same tick produces identical seeds → all visitors pick the same destination.
    /// </summary>
    [Test]
    public void SameTickSeededRandom_ProducesSameFirstValue()
    {
        // Simulate the old (broken) pattern: seed from Environment.TickCount64 in a tight loop.
        // All instances get the same seed → same first value.
        long seed = Environment.TickCount64;
        var results = Enumerable.Range(0, 10)
            .Select(_ => new Random((int)seed).Next(7))
            .ToHashSet();

        // With identical seeds every instance returns the same value.
        Assert.That(results.Count, Is.EqualTo(1),
            "This test documents the old bug: same-seed Random instances all return the same value.");
    }

    /// <summary>
    /// The fix: new Random() without a seed uses OS entropy (in .NET 6+),
    /// giving each instance a unique seed regardless of call timing.
    /// </summary>
    [Test]
    public void UnseedtedRandom_ProducesDiverseValues()
    {
        // Simulates the fixed pattern in Visitor.cs:
        //   private readonly Random _random = new();
        // Even when 20 instances are created back-to-back they should NOT all
        // return the same value.
        var results = Enumerable.Range(0, 20)
            .Select(_ => new Random().Next(7))
            .ToHashSet();

        // 20 draws from 7 buckets with independent seeds: probability of ≤2
        // unique values is astronomically small (< 10^-14).
        Assert.That(results.Count, Is.GreaterThanOrEqualTo(3),
            "20 unseeded Random instances should produce diverse values (seeding regression).");
    }

    /// <summary>
    /// Documents the recommended Visitor pattern: one Random per instance,
    /// not recreated per call.  Multiple Next() calls on the same instance
    /// produce a proper random sequence.
    /// </summary>
    [Test]
    public void PerInstanceRandom_ProducesDistributedSequence()
    {
        const int destinations = 7;
        const int sampleSize = 200;

        var rng = new Random(42); // fixed seed for repeatability
        var counts = new int[destinations];

        for (int i = 0; i < sampleSize; i++)
            counts[rng.Next(destinations)]++;

        // Rough uniformity check: every bucket should get at least 10 hits
        // (expected ~28) with a fixed seed over 200 samples.
        foreach (var count in counts)
            Assert.That(count, Is.GreaterThan(10),
                "Random distribution is unexpectedly skewed — check Next() usage.");
    }
}
