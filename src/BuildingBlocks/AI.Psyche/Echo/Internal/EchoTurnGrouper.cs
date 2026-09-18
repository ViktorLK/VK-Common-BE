using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Echo.Internal;

/// <summary>
/// Helper utility for grouping conversation echo metadata or fragments into turn exchanges.
/// Groups messages from newest to oldest based on <see cref="VKChatRole.User"/> turn boundaries.
/// </summary>
internal static class EchoTurnGrouper
{
    /// <summary>
    /// Groups chronological messages into turns, walking backwards from newest to oldest.
    /// Each turn retains its internal chronological order.
    /// </summary>
    /// <typeparam name="T">The echo message type.</typeparam>
    /// <param name="echoes">The list of chronological echo messages.</param>
    /// <param name="roleSelector">The selector function to determine the chat role of a message.</param>
    /// <returns>A list of turns (from newest turn to oldest turn), each containing messages in chronological order.</returns>
    public static List<List<T>> Group<T>(IReadOnlyList<T> echoes, Func<T, VKChatRole> roleSelector)
    {
        VKGuard.NotNull(echoes); // [AP.01]
        VKGuard.NotNull(roleSelector); // [AP.01]

        var turns = new List<List<T>>();
        if (echoes.Count == 0)
        {
            return turns;
        }

        var currentTurn = new List<T>();

        // Walk backwards from latest to oldest
        for (int i = echoes.Count - 1; i >= 0; i--)
        {
            var msg = echoes[i];
            currentTurn.Add(msg);

            // A User turn marker completes a conversational turn exchange
            if (roleSelector(msg) == VKChatRole.User)
            {
                currentTurn.Reverse(); // In-place reverse restores chronological order within the turn
                turns.Add(currentTurn);
                currentTurn = [];
            }
        }

        if (currentTurn.Count > 0)
        {
            currentTurn.Reverse();
            turns.Add(currentTurn);
        }

        return turns;
    }
}
