using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AGS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int GameWidth = 50;
        int GameHeight = 50;

        Dictionary<string, string> keyexec = new Dictionary<string, string>();
        Dictionary<string, string> whenexec = new Dictionary<string, string>();
        Dictionary<string, string> vars = new Dictionary<string, string>();
        int logseverity = 0;

        public MainWindow()
        {
            InitializeComponent();
            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.RealTime;
            LoadConfig("config.age");
        }

        void LoadConfig(string configname)
        {
            if (File.Exists(configname))
            {
                var content = File.ReadAllLines(configname);
                GameHeight = ParseInt(content[0]);
                GameWidth = ParseInt(content[1]);
                if (content.Count() == 3)
                {
                    loadcode(content[2]);
                }
                else if (content.Count() == 4)
                {
                    loadcode(content[2]);
                    logseverity = ParseInt(content[3]);
                }
            }
            else
            {
                error("Config file " + configname + " doesn't exist.");
            }
        }

        void drawpixel(int x, int y, Color color)
        {
            log("Drawing pixel with color " + color.ToString() + " to X:" + x + " Y:" + y, 2);
            Rectangle rect = new Rectangle()
            {
                Stroke = new SolidColorBrush(color),
                Fill = new SolidColorBrush(color),
                Width = GameCanvas.Width / GameWidth,
                Height = GameCanvas.Height / GameHeight
            };
            Canvas.SetLeft(rect, x * GameCanvas.Width / GameWidth);
            Canvas.SetTop(rect, y * GameCanvas.Height / GameHeight);
            GameCanvas.Children.Add(rect);
        }

        void log(string text, int severity)
        {
            if ((severity == 1 && logseverity > 0) || (severity == 0 && logseverity != 1) || (logseverity == 2))
            {
                File.AppendAllText("ardagameengine.log", DateTime.UtcNow + "(UTC): " + text + Environment.NewLine);
            }
        }

        void error(string text, bool savetolog = true)
        {
            if (savetolog) { log(text, 0); }
            vars.Remove("error");
            vars.Add("error", text);
            loadcode("error.age"); //draw error texture etc
        }

        void loadtexture(int x, int y, string filename)
        {
            log("Loading Texture with filename " + filename + " to X:" + x + " Y:" + y, 2);
            if (File.Exists(filename))
            {
                drawtexture(x, y, File.ReadAllText(filename, Encoding.UTF8));
            }
            else
            {
                error("File not found: " + filename);
            }
        }

        void drawtexture(int x, int y, string texture)
        {
            log("Drawing Texture with texture " + texture + " to X:" + x + " Y:" + y, 2);
            List<string> todraw = texture.Split('-').ToList<string>();

            var workingx = x;
            var workingy = y;

            if (todraw.Count == 1)
            {
                error("One pixel received, if you want to draw only one pixel, use drawpixel");
                return;
            }
            var currentcolor = Colors.Black;
            foreach (string drawcommand in todraw)
            {
                if (drawcommand.Trim() == "nl")
                {
                    workingy++;
                    //new line
                    workingx = x;
                }
                else if (drawcommand.Trim().StartsWith("sc"))
                {
                    //setcolor
                    currentcolor = (Color)ColorConverter.ConvertFromString(drawcommand.Replace("sc", ""));
                }
                else if (drawcommand.Trim() == "1")
                {
                    drawpixel(workingx, workingy, currentcolor);
                    workingx++;
                }
                else if (drawcommand.Trim() == "0")
                {
                    workingx++;
                }
            }
        }

        void loadcode(string filename)
        {
            log("Loading code with filename " + filename, 2);
            if (File.Exists(filename))
            {
                runcode(File.ReadAllText(filename, Encoding.UTF8));
            }
            else
            {
                error("File not found: " + filename);
            }
        }

        void drawtext(int x, int y, int size, string text)
        {
            log("Drawing text (not implemented) " + text + " to X:" + x + " Y:" + y, 2);
            throw new NotImplementedException();
        }

        void runcode(string code)
        {
            log("Running code " + code, 2);
            List<string> ToExecute = code.Split(';').ToList<string>();
            foreach (string CodeLine in ToExecute)
            {
                var CodeToRun = CodeLine;
                if (CodeToRun.IndexOf("//") != -1)
                {
                    CodeToRun = CodeToRun.Substring(0, CodeToRun.IndexOf("//"));
                }
                CodeToRun = CodeToRun.Trim();

                if (CodeToRun != "")
                {
                    var StartIndex = CodeToRun.IndexOf("(") + 1;
                    var variables = (CodeToRun.Substring(StartIndex, CodeToRun.LastIndexOf(")") - StartIndex));
                    
                    foreach (string varname in vars.Keys)
                    {
                        var value = "";
                        vars.TryGetValue(varname, out value);
                        variables = variables.Replace("$" + varname, value);
                    }

                    while (variables.Contains("Add("))
                    {
                        var CodeToRun2 = variables.Substring(variables.IndexOf("Add("));
                        var StartIndexvar = CodeToRun2.IndexOf("(") + 1;
                        var variablesvar = (CodeToRun2.Substring(StartIndexvar, CodeToRun2.LastIndexOf(")") - StartIndexvar));
                        var variablepartsvar = variablesvar.Split(',');
                        variables = variables.Replace(CodeToRun2.Substring(0, CodeToRun2.IndexOf(")")), (ParseInt(variablepartsvar[0].Replace("(", "").Replace(")", "").Trim()) + ParseInt(variablepartsvar[1].Replace("(", "").Replace(")", "").Trim())).ToString());
                    }

                    while (variables.Contains("Multiply("))
                    {
                        var CodeToRun2 = variables.Substring(variables.IndexOf("Multiply("));
                        var StartIndexvar = CodeToRun2.IndexOf("(") + 1;
                        var variablesvar = (CodeToRun2.Substring(StartIndexvar, CodeToRun2.LastIndexOf(")") - StartIndexvar));
                        var variablepartsvar = variablesvar.Split(',');
                        variables = variables.Replace(CodeToRun2.Substring(0, CodeToRun2.IndexOf(")")), (ParseInt(variablepartsvar[0].Replace("(", "").Replace(")", "").Trim()) * ParseInt(variablepartsvar[1].Replace("(", "").Replace(")", "").Trim())).ToString());
                    }

                    while (variables.Contains("Divide("))
                    {
                        var CodeToRun2 = variables.Substring(variables.IndexOf("Divide("));
                        var StartIndexvar = CodeToRun2.IndexOf("(") + 1;
                        var variablesvar = (CodeToRun2.Substring(StartIndexvar, CodeToRun2.LastIndexOf(")") - StartIndexvar));
                        var variablepartsvar = variablesvar.Split(',');
                        variables = variables.Replace(CodeToRun2.Substring(0, CodeToRun2.IndexOf(")")), (ParseInt(variablepartsvar[0].Replace("(", "").Replace(")", "").Trim()) / ParseInt(variablepartsvar[1].Replace("(", "").Replace(")", "").Trim())).ToString());
                    }

                    var variableparts = variables.Split(',');
                    var thecommand = CodeToRun.Substring(0, CodeToRun.IndexOf("("));

                    switch (thecommand)
                    {
                        case ("MessageBox"):
                            MessageBox.Show(variables);
                            break;
                        case ("Log"):
                            log(variables,1);
                            break;
                        case ("LoadCode"):
                            loadcode(variables);
                            break;
                        case ("WaitForRender"):
                            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => { })).Wait();
                            break;
                        case ("Wait"):
                            System.Threading.Thread.Sleep(int.Parse(variables));
                            break;
                        case ("ClearScreen"):
                            GameCanvas.Children.Clear();
                            break;
                        case ("Clear"):
                            GameCanvas.Children.Clear();
                            vars.Clear();
                            break;
                        case ("DrawTexture"):
                            drawtexture(ParseInt(variableparts[0]), ParseInt(variableparts[1]), variableparts[2]);
                            break;
                        case ("LoadTexture"):
                            loadtexture(ParseInt(variableparts[0]), ParseInt(variableparts[1]), variableparts[2]);
                            break;
                        case ("DrawPixel"):
                            drawpixel(ParseInt(variableparts[0]), ParseInt(variableparts[1]), (Color)ColorConverter.ConvertFromString(variableparts[2]));
                            break;
                        case ("OnKey"):
                            keyexec.Add(variableparts[0], variableparts[1]);
                            break;
                        case ("when"):
                            whenexec.Add(variableparts[0] + "==" + variableparts[1], variableparts[2]);
                            break;
                        case ("SetVariable"):
                            vars.Remove(variableparts[0]);
                            vars.Add(variableparts[0], variableparts[1]);
                            RefreshDebug();
                            var toexec = "";
                            if (whenexec.TryGetValue(variableparts[0] + "==" + variableparts[1], out toexec))
                            {
                                whenexec.Remove(variableparts[0] + "==" + variableparts[1]);
                                loadcode(variableparts[0] + "==" + variableparts[1]);
                            }
                            break;
                        case ("ClearVariables"):
                            vars.Clear();
                            break;
                        case ("LoadConfig"):
                            LoadConfig(variableparts[0]);
                            break;
                        case ("if"):
                            if (variableparts[0].Trim() == variableparts[1].Trim())
                            {
                                loadcode(variableparts[2].Trim());
                            }
                            break;
                        case ("while"):
                            while ((variableparts[0].Trim().StartsWith("$") ? vars[variableparts[0].Trim()] : variableparts[0].Trim()) == (variableparts[1].Trim().StartsWith("$") ? vars[variableparts[1].Trim()] : variableparts[1].Trim()))
                            {
                                loadcode(variableparts[2].Trim());
                            }
                            break;
                        case ("ifnot"):
                            if (variableparts[0].Trim() != variableparts[1].Trim())
                            {
                                loadcode(variableparts[2].Trim());
                            }
                            break;
                        case ("ifc"):
                            if (variableparts[0].Trim() == variableparts[1].Trim())
                            {
                                runcode(variables.Substring(variables.IndexOf(variableparts[2].Trim())));
                            }
                            break;
                        case ("ifcnot"):
                            if (variableparts[0].Trim() != variableparts[1].Trim())
                            {
                                loadcode(variableparts[2].Trim());
                            }
                            break;
                        case ("for"):
                            for (int i = 0; i > ParseInt(variableparts[0]); i++)
                            {
                                vars.Remove("forvalue");
                                vars.Add("forvalue", i.ToString());

                                loadcode(variableparts[1].Trim());
                            }
                            break;
                    }
                }
            }
        }


        int ParseInt(string text)
        {
            int x = -1;
            if (int.TryParse(text, out x))
            {
                log("int parse successful, int: " + x, 2);
            }
            else
            {
                log("int parse failed, int: " + text,2);
            }
            return x;
        }

        void RefreshDebug()
        {
            textBlock.Text = "";
            foreach (string varname in vars.Keys)
            {
                var value = "";
                vars.TryGetValue(varname, out value);
                textBlock.Text += "$" + varname + " : " + value + Environment.NewLine;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            log("Key down: " + e.Key.ToString().ToLower(), 2);
            var script = "";
            if (keyexec.TryGetValue(e.Key.ToString().ToLower(), out script))
            {
                vars.Remove("pressedkey");
                vars.Add("pressedkey", e.Key.ToString().ToLower());
                loadcode(script);
            }
        }
    }
}