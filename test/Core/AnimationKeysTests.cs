using System.Collections.Generic;
using NUnit.Framework;
using dd2d.core;

namespace dd2d.Tests.Core;

/// <summary>
/// Tests for AnimationKeys constants.
///
/// Purpose: AnimationKeys is the single source of truth for animation names.
/// A typo here silently breaks animations at runtime — these tests catch
/// regressions at build time instead.
///
/// No Godot runtime required — pure constant verification.
/// </summary>
[TestFixture]
public class AnimationKeysTests
{
    // ── Naming contract ─────────────────────────────────────────────────────
    // All animation names must be lowercase camelCase so they match the
    // keys defined in character.tscn / visitor.tscn AnimationLibrary nodes.

    [Test]
    public void WalkForward_IsLowercaseCamelCase()
        => Assert.That(AnimationKeys.WalkForward, Is.EqualTo("walkF"));

    [Test]
    public void WalkBack_IsLowercaseCamelCase()
        => Assert.That(AnimationKeys.WalkBack, Is.EqualTo("walkB"));

    [Test]
    public void WalkLeft_IsLowercaseCamelCase()
        => Assert.That(AnimationKeys.WalkLeft, Is.EqualTo("walkL"));

    [Test]
    public void WalkRight_IsLowercaseCamelCase()
        => Assert.That(AnimationKeys.WalkRight, Is.EqualTo("walkR"));

    [Test]
    public void Idle_MatchesSceneKey()
        => Assert.That(AnimationKeys.Idle, Is.EqualTo("idle"));

    [Test]
    public void Interact_MatchesSceneKey()
        => Assert.That(AnimationKeys.Interact, Is.EqualTo("interact"));

    [Test]
    public void Sit_MatchesSceneKey()
        => Assert.That(AnimationKeys.Sit, Is.EqualTo("sit"));

    [Test]
    public void StandUp_MatchesSceneKey()
        => Assert.That(AnimationKeys.StandUp, Is.EqualTo("stand_up"));

    // ── No-empty-string guard ────────────────────────────────────────────────
    // An empty animation name passes at compile time but causes a silent
    // no-op (or an AnimationPlayer warning) at runtime.

    [TestCase(AnimationKeys.WalkForward)]
    [TestCase(AnimationKeys.WalkBack)]
    [TestCase(AnimationKeys.WalkLeft)]
    [TestCase(AnimationKeys.WalkRight)]
    [TestCase(AnimationKeys.Idle)]
    [TestCase(AnimationKeys.Interact)]
    [TestCase(AnimationKeys.Sit)]
    [TestCase(AnimationKeys.StandUp)]
    public void AllKeys_AreNonEmpty(string key)
        => Assert.That(key, Is.Not.Null.And.Not.Empty);

    // ── No-duplicate guard ───────────────────────────────────────────────────
    // Two different constants resolving to the same string usually means a
    // copy-paste error (e.g. WalkLeft accidentally set to "walkR").

    [Test]
    public void AllKeys_AreUnique()
    {
        var all = new[]
        {
            AnimationKeys.WalkForward,
            AnimationKeys.WalkBack,
            AnimationKeys.WalkLeft,
            AnimationKeys.WalkRight,
            AnimationKeys.Idle,
            AnimationKeys.Interact,
            AnimationKeys.Sit,
            AnimationKeys.StandUp,
        };

        var distinct = new HashSet<string>(all);
        Assert.That(distinct.Count, Is.EqualTo(all.Length),
            "Two AnimationKeys constants share the same string value — likely a copy-paste error.");
    }
}
