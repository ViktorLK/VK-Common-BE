using System;
using System.Collections.Generic;
using VK.Blocks.AI;

namespace VK.Blocks.AI.Cognitive.Intent.Internal;

/// <summary>
/// Builds the LLM prompt for intent and triage extraction.
/// Handles both structured JSON output and free-text fallback prompt variants.
/// Follows AP.03 (internal, no VK prefix).
/// </summary>
internal static class IntentPromptBuilder // [AP.03] Internal, no VK prefix
{
    private const string SystemPromptStructured =
        """
        You are a fast intent and context triage classifier. Analyze the user's input and recent conversation history.
        Output a JSON object with these fields (in this exact order):
        - "intent": one of [Chat, Roleplay, Consulting, Task, System]
        - "confidence": float between 0.0 and 1.0
        - "emotion": primary detected user emotion (e.g., "Happy", "Sad", "Anxious", "Angry", "Frustrated", "Neutral")
        - "emotion_intensity": integer between 1 and 10 indicating emotion intensity
        - "complexity": one of ["Simple", "Complex"]
        - "urgency": one of ["Low", "Medium", "High"]
        - "topic": short topic/domain keyword (e.g., "Technical", "Billing", "Account", "General")
        - "language": ISO language code (e.g., "zh", "ja", "en")
        - "requires_knowledge": boolean (true if external knowledge or memory retrieval is needed, false otherwise)
        - "safety_category": one of ["Safe", "Sensitive", "Harmful"]
        - "query_cue": a refined search query for memory/corpus retrieval (null if not applicable)
        - "persona_hint": persona context hint if the input suggests a specific persona (null if not applicable)
        - "refined_input": cleaned/refined version of the user's message

        Intent definitions:
        - Chat: General conversation, greeting, small talk, chit-chat
        - Roleplay: Acting out a scenario, character-based interaction, creative writing
        - Consulting: Asking for advice, searching for facts, professional consultation, venting
        - Task: Managing tasks, projects, scheduling, administrative actions
        - System: Internal system commands or meta-questions about the AI
        """;

    private const string SystemPromptFreeText =
        """
        You are a fast intent and context triage classifier. Analyze the user's input and recent conversation history.
        Output in this exact format (pipe-separated):
        [Intent] | [Confidence] | [Emotion] | [EmotionIntensity] | [Complexity] | [Urgency] | [Topic] | [Language] | [RequiresKnowledge] | [SafetyCategory] | [QueryCue] | [PersonaHint] | [RefinedInput]

        Intent must be one of: Chat, Roleplay, Consulting, Task, System
        Confidence: float between 0.0 and 1.0
        Emotion: primary user emotion (e.g. Neutral, Frustrated, Happy)
        EmotionIntensity: integer 1-10
        Complexity: Simple or Complex
        Urgency: Low, Medium, or High
        Topic: short domain tag (e.g. Technical, General)
        Language: language code (zh, ja, en)
        RequiresKnowledge: true or false
        SafetyCategory: Safe, Sensitive, or Harmful
        QueryCue: refined search query (or empty)
        PersonaHint: persona hint (or empty)
        RefinedInput: cleaned user message

        Example: Consulting | 0.85 | Frustrated | 7 | Complex | Medium | Technical | en | true | Safe | quantum computing basics | science_tutor | Tell me about quantum computing
        """;

    /// <summary>
    /// Builds the message array for structured JSON output intent extraction.
    /// </summary>
    public static VKChatMessage[] BuildStructuredPrompt(
        string input,
        IEnumerable<VKChatMessage>? recentHistory,
        int maxHistoryMessages)
    {
        return BuildMessages(SystemPromptStructured, input, recentHistory, maxHistoryMessages);
    }

    /// <summary>
    /// Builds the message array for free-text fallback intent extraction.
    /// </summary>
    public static VKChatMessage[] BuildFreeTextPrompt(
        string input,
        IEnumerable<VKChatMessage>? recentHistory,
        int maxHistoryMessages)
    {
        return BuildMessages(SystemPromptFreeText, input, recentHistory, maxHistoryMessages);
    }

    private static VKChatMessage[] BuildMessages(
        string systemPrompt,
        string input,
        IEnumerable<VKChatMessage>? recentHistory,
        int maxHistoryMessages)
    {
        var messages = new List<VKChatMessage>
        {
            VKChatMessage.FromText(VKChatRole.System, systemPrompt)
        };

        // Append trimmed recent history for context (prefill trimming)
        if (recentHistory is not null)
        {
            int count = 0;
            foreach (var msg in recentHistory)
            {
                if (count >= maxHistoryMessages)
                {
                    break;
                }
                messages.Add(msg);
                count++;
            }
        }

        // Append current user input
        messages.Add(VKChatMessage.FromText(VKChatRole.User, $"Classify this input:\n{input}"));

        return [.. messages];
    }

    /// <summary>
    /// Parses a free-text pipe-separated LLM response into an <see cref="VKIntentContext"/>.
    /// Returns null if parsing fails completely.
    /// </summary>
    public static VKIntentContext? ParseFreeTextResponse(string content, string originalInput)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        string[] parts = content.Split('|', StringSplitOptions.TrimEntries);
        if (parts.Length < 1)
        {
            return null;
        }

        if (!Enum.TryParse<VKIntent>(parts[0], ignoreCase: true, out var intent))
        {
            intent = VKIntent.Chat;
        }

        double confidence = 0.8;
        if (parts.Length > 1 && double.TryParse(parts[1], out var parsedConfidence))
        {
            confidence = Math.Clamp(parsedConfidence, 0.0, 1.0);
        }

        string? emotion = parts.Length > 2 && !string.IsNullOrWhiteSpace(parts[2]) ? parts[2] : null;
        int? emotionIntensity = null;
        if (parts.Length > 3 && int.TryParse(parts[3], out var parsedIntensity))
        {
            emotionIntensity = Math.Clamp(parsedIntensity, 1, 10);
        }

        string? complexity = parts.Length > 4 && !string.IsNullOrWhiteSpace(parts[4]) ? parts[4] : "Simple";
        string? urgency = parts.Length > 5 && !string.IsNullOrWhiteSpace(parts[5]) ? parts[5] : "Medium";
        string? topic = parts.Length > 6 && !string.IsNullOrWhiteSpace(parts[6]) ? parts[6] : null;
        string? language = parts.Length > 7 && !string.IsNullOrWhiteSpace(parts[7]) ? parts[7] : null;

        bool requiresKnowledge = false;
        if (parts.Length > 8 && bool.TryParse(parts[8], out var parsedRequiresKnowledge))
        {
            requiresKnowledge = parsedRequiresKnowledge;
        }

        string? safetyCategory = parts.Length > 9 && !string.IsNullOrWhiteSpace(parts[9]) ? parts[9] : "Safe";
        string? queryCue = parts.Length > 10 && !string.IsNullOrWhiteSpace(parts[10]) ? parts[10] : null;
        string? personaHint = parts.Length > 11 && !string.IsNullOrWhiteSpace(parts[11]) ? parts[11] : null;
        string? refinedInput = parts.Length > 12 && !string.IsNullOrWhiteSpace(parts[12]) ? parts[12] : originalInput;

        return new VKIntentContext
        {
            Intent = intent,
            Confidence = confidence,
            Emotion = emotion,
            EmotionIntensity = emotionIntensity,
            Complexity = complexity,
            Urgency = urgency,
            Topic = topic,
            Language = language,
            RequiresKnowledge = requiresKnowledge,
            SafetyCategory = safetyCategory,
            QueryCue = queryCue,
            PersonaHint = personaHint,
            RefinedInput = refinedInput ?? originalInput,
            SchemaVersion = 1,
            Source = "LLM.FreeText"
        };
    }
}
