using System;
using System.Net.Http;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text;

//using JsonConverter = Newtonsoft.Json.JsonConvert;
namespace t8
{
    public partial class Form1 : Form
    {
        private const string apiUrl = "http://api.tvmaze.com/search/shows";
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string searchText = textBox1.Text;

            Task.Run(async () =>
            {
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        string requestUrl = $"{apiUrl}?q={searchText}";
                        HttpResponseMessage response = await client.GetAsync(requestUrl);
                        response.EnsureSuccessStatusCode();

                        string responseBody = await response.Content.ReadAsStringAsync();
                        

                        // Clean the response text
                        string cleanedText = CleanResponseText(responseBody);

                        // Format the cleaned text with indentation
                        string formattedText = FormatJson(cleanedText);

                        // Display the formatted response
                        ShowFormattedResponse(formattedText);
                    }
                    catch (Exception ex)
                    {
                        ShowError(ex.Message);
                    }
                }
            });
        }

        private string CleanResponseText(string text)
        {
            // Remove square brackets [], curly braces {}, and HTML tags
            string cleanedText = Regex.Replace(text, @"\[|\]|\{|\}|<.*?>", string.Empty);

            // Remove excessive whitespace
            cleanedText = Regex.Replace(cleanedText, @"\s+", " ");

            return cleanedText;
        }

       private string FormatJson(string json)
        {
            try
            {
                var result = new StringBuilder();
                int indentLevel = 0;
                bool inQuotes = false;
                bool isEscaped = false;

                foreach (char character in json)
                {
                    switch (character)
                    {
                        case '{':
                        case '[':
                            result.Append(character);
                            if (!inQuotes)
                            {
                                result.AppendLine();
                                result.Append('\t', ++indentLevel);
                            }
                            break;
                        case '}':
                        case ']':
                            if (!inQuotes)
                            {
                                result.AppendLine();
                                result.Append('\t', --indentLevel);
                            }
                            result.Append(character);
                            break;
                        case ',':
                            result.Append(character);
                            if (!inQuotes)
                            {
                                result.AppendLine();
                                result.Append('\t', indentLevel);
                            }
                            break;
                        case ':':
                            result.Append(character);
                            if (!inQuotes)
                                result.Append(' ');
                            break;
                        case '"':
                            result.Append(character);
                            if (!isEscaped)
                                inQuotes = !inQuotes;
                            break;
                        case '\r':
                        case '\n':
                        case '\t':
                        case ' ':
                            if (inQuotes)
                                result.Append(character);
                            else
                                result.Append(character).Append(' '); // Adăugați spațiul suplimentar aici
                            break;
                        default:
                            result.Append(character);
                            break;
                    }
                    isEscaped = character == '\\' && !isEscaped;
                }

                return result.ToString();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                return string.Empty;
            }
        }
        private void ShowFormattedResponse(string formattedText)
        {
            // Run on UI thread to update the UI controls
            Invoke((MethodInvoker)(() => textBox1.Text = formattedText));
        }

        private void ShowError(string errorMessage)
        {
            // Run on UI thread to show the error message
            Invoke((MethodInvoker)(() => MessageBox.Show($"Error: {errorMessage}")));
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
    
}
