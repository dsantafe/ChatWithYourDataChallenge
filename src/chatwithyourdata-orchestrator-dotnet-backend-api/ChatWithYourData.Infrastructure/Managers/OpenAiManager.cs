namespace ChatWithYourData.Infrastructure.Managers
{
    using Azure.AI.OpenAI;
    using Microsoft.DeepDev;
    using OpenAI.Chat;
    using System;
    using System.ClientModel;
    using System.Text.Json;

    public class OpenAiManager(string endpoint, string apiKey)
    {
        private readonly AzureOpenAIClient _client = new(
                new Uri(endpoint),
                new ApiKeyCredential(apiKey));

        public Tuple<string, int, int> GenerateSqlQuery(
            string deploymentName,
            string prompt,
            string template)
        {
            ChatClient chatClient = _client.GetChatClient(deploymentName);
            ChatCompletionOptions chatCompletionOptions = new()
            {
                MaxOutputTokenCount = 1024,
                Temperature = 0,
                TopP = 1
            };
            ChatMessage[] messages = [new SystemChatMessage(template), new UserChatMessage(prompt)];
            ChatCompletion chatCompletion = chatClient.CompleteChat(messages, chatCompletionOptions);
            string sqlQuery = chatCompletion.Content[0].Text;

            int tokenCountInp = GetTokenizerTokenCount(messages);
            int tokenCountOutp = GetTokenizerTokenCount([new SystemChatMessage(sqlQuery)]);

            return new Tuple<string, int, int>(sqlQuery, tokenCountInp, tokenCountOutp);
        }

        public static int GetTokenizerTokenCount(
            ChatMessage[] messages)
        {
            ITokenizer tokenizer = TokenizerBuilder.CreateByModelNameAsync("gpt-3.5-turbo").GetAwaiter().GetResult();
            List<int> tokenCount = tokenizer.Encode(JsonSerializer.Serialize(messages), []);
            return tokenCount.Count;
        }

        public static int GetTokenizerTokenCount(
            string prompt,
            string template)
        {
            ChatMessage[] messages = [new SystemChatMessage(template), new UserChatMessage(prompt)];
            ITokenizer tokenizer = TokenizerBuilder.CreateByModelNameAsync("gpt-3.5-turbo").GetAwaiter().GetResult();
            List<int> tokenCount = tokenizer.Encode(JsonSerializer.Serialize(messages), []);
            return tokenCount.Count;
        }

        public static int GetSharpTokenCount(
            ChatMessage[] messages)
        {
            const int TokensPerMessage = 3;
            const int TokensPerRole = 1;
            const int BaseTokens = 3;
            var disallowedSpecial = new HashSet<string>();

            var tokenCount = BaseTokens;
            var encoding = SharpToken.GptEncoding.GetEncoding("cl100k_base");
            foreach (var message in messages)
            {
                tokenCount += TokensPerMessage;
                tokenCount += TokensPerRole;
                tokenCount += encoding.Encode(JsonSerializer.Serialize(message), disallowedSpecial).Count;
            }

            return tokenCount;
        }
    }
}
