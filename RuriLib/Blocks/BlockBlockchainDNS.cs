using Extreme.Net;
using RuriLib.LS;
using RuriLib.Models;
using System.Windows.Media;

namespace RuriLib
{
    /// <summary>
    /// A block that solves a reCaptcha challenge.
    /// </summary>
    public class BlockBlockchainDNS : BlockBase
    {
        private string variableName = "";

        /// <summary>The name of the output variable where the challenge solution will be stored.</summary>
        public string VariableName { get { return variableName; } set { variableName = value; OnPropertyChanged(); } }

        private string url = "https://google.com";

        /// <summary>The URL attempting to be resolved.</summary>
        public string Url { get { return url; } set { url = value; OnPropertyChanged(); } }

        /// <summary>
        /// Creates a reCaptcha block.
        /// </summary>
        public BlockBlockchainDNS()
        {
            Label = "BLOCKCHAINDNS";
        }

        /// <inheritdoc />
        public override BlockBase FromLS(string line)
        {
            // Trim the line
            var input = line.Trim();

            // Parse the label
            if (input.StartsWith("#"))
                Label = LineParser.ParseLabel(ref input);

            Url = LineParser.ParseLiteral(ref input, "URL");

            if (LineParser.ParseToken(ref input, TokenType.Arrow, false) == "")
                return this;

            LineParser.EnsureIdentifier(ref input, "VAR");

            // Parse the variable name
            VariableName = LineParser.ParseLiteral(ref input, "VARIABLE NAME");

            return this;
        }

        /// <inheritdoc />
        public override string ToLS(bool indent = true)
        {
            var writer = new BlockWriter(GetType(), indent, Disabled);
            writer
                .Label(Label)
                .Token("BLOCKCHAINDNS")
                .Literal(Url)
                .Arrow()
                .Token("VAR")
                .Literal(VariableName);

            return writer.ToString();
        }

        /// <inheritdoc />
        public override void Process(BotData data)
        {
            if (!data.GlobalSettings.Captchas.BypassBalanceCheck)
                base.Process(data);

            data.Log(new LogEntry("Resolving DNS...", Colors.White));

            HttpRequest request = new HttpRequest();

            HttpResponse newUrl = request.Raw(HttpMethod.GET, "https://bdns.co/r/" + ReplaceValues(url, data));
            string save = newUrl.ToString().Trim();

            //MessageBox.Show(response.ToString());

            data.Log(newUrl.HasError ? new LogEntry("Failed to resolve " + url, Colors.Tomato) : new LogEntry("Succesfully resolved: " + url, Colors.GreenYellow));

            var timeout = data.GlobalSettings.General.RequestTimeout * 1000;
            request.IgnoreProtocolErrors = true;
            request.AllowAutoRedirect = false;
            request.EnableEncodingContent = true;
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";
            request.ReadWriteTimeout = timeout;
            request.ConnectTimeout = timeout;
            request.KeepAlive = true;
            request.MaximumAutomaticRedirections = data.ConfigSettings.MaxRedirects;

            HttpResponse response = request.Raw(HttpMethod.GET, save);

            data.Log(new LogEntry("Response Source: ", Colors.Green));

            data.ResponseSource = response.ToString();
            data.Log(new LogEntry(data.ResponseSource, Colors.GreenYellow));

            data.Log(new LogEntry("Resolved URL stored in variable: " + variableName, Colors.White));
            data.Variables.Set(new CVar(variableName, save));
        }
    }
}