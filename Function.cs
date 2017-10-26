using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization;
using AWSLambdaAutoInsurance.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AWSLambdaAutoInsurance
{
    public class Function
    {

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static HttpClient _httpClient;
        public const string INVOCATION_NAME = "Auto Insurance";

        public Function()
        {
            _httpClient = new HttpClient();
        }

        public async Task<SkillResponse> FunctionHandler(SkillRequest input, ILambdaContext context)
        {

            var requestType = input.GetRequestType();
            if (requestType == typeof(IntentRequest))
            {
                var intentRequest = input.Request as IntentRequest;
                var stateRequested = intentRequest.Intent.Slots["State"].Value;

                if (stateRequested == null)
                {
                    context.Logger.LogLine($"The state was not understood.");
                    return MakeSkillResponse("I'm sorry, but I didn't understand the state you were asking for. Please ask again.", false);
                }

                Rater rate = new Rater();

                var avgRate = rate.GetAverageRateForState(stateRequested);

                if(avgRate.Count == 0)
                {
                    return MakeSkillResponse(
                       $"No data found for {stateRequested}",
                       true);
                }

                var outputText = $"For {stateRequested} state, the carrier with lowest average premium is {avgRate.First().Key} and average rate is {avgRate.First().Value.Substring(0,5)} dollars. The average rates are based on quotes made for last 3 months";
                return MakeSkillResponse(
                        outputText,
                        true);
            }
            else
            {
                return MakeSkillResponse(
                        $"I don't know how to handle this intent. Please say something like Alexa, ask {INVOCATION_NAME} about California.",
                        true);
            }
        }


        private SkillResponse MakeSkillResponse(string outputSpeech,
            bool shouldEndSession,
            string repromptText = "Just say, tell me about California to learn more. To exit, say, exit.")
        {
            var response = new ResponseBody
            {
                ShouldEndSession = shouldEndSession,
                OutputSpeech = new PlainTextOutputSpeech { Text = outputSpeech }
            };

            if (repromptText != null)
            {
                response.Reprompt = new Reprompt() { OutputSpeech = new PlainTextOutputSpeech() { Text = repromptText } };
            }

            var skillResponse = new SkillResponse
            {
                Response = response,
                Version = "1.0"
            };
            return skillResponse;
        }

    }
}
