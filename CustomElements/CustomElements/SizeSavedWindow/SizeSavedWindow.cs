using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Xml;

namespace CustomElements.SizeSavedWindow
{
    public class SizeSavedWindow
    {
        //To keep a window at the same size and position, just add 
        // SizeSavedWindow.addToSizeSavedWindows(this);
        // right after initialieComponent
        public static void addToSizeSavedWindows(Window window)
        {
            //System.Console.Write("added to size saved Windows\n");


            window_IsLoaded(window, null);
            //window.Loaded += window_IsLoaded;
            window.Closed += window_Closing;
        }

        static void window_Closing(object sender, EventArgs e)
        {
            
            Window realSender = (Window)sender;
            //realSender.RestoreBounds
            //System.Console.WriteLine("CLOSING!!!" + realSender.Name + "<<");
            windowValuesChanged(realSender);
        }

        static void window_IsLoaded(object sender, RoutedEventArgs e)
        {
            //System.Console.Write("initialized to size saved Windows\n");
            Window window = (Window)sender;
            /*if (!(window.Visibility == Visibility.Visible))
            {
                return;
            }*/
            bool restoredSizes = false;
            if (File.Exists("sizes.xml"))
            {
                var stream = new FileStream("sizes.xml", FileMode.Open);
                try
                {
                    var reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas());
                    var deserializer = new DataContractSerializer(typeof(Dictionary<string, double[]>));
                    //System.Console.Write("a\n");
                    windows = (Dictionary<string, double[]>)deserializer.ReadObject(reader, true);


                    foreach (KeyValuePair<string, double[]> pair in windows)
                    {
                        //System.Console.Write("Load Window.Title: {0}\n", pair.Key);
                        if (pair.Key == window.Title)
                        {
                            //System.Console.WriteLine("{4} : {0} - {1} - {2} - {3}\n", pair.Value[0], pair.Value[1], pair.Value[2], pair.Value[3], window.Title);
                            //THIS WINDOW IS ALREADY IN THE FILE
                            window.Height = pair.Value[0];
                            window.Width = pair.Value[1];
                            window.Top = pair.Value[2];
                            window.Left = pair.Value[3];
                            break;
                        }
                        //load file and set window positions and sizes
                        //window.Height = 700;
                        //window.Width = 700;
                        //Check if window out of screen or too big
                    }
                    restoredSizes = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("error - could not restore windows sizes - " + ex.GetBaseException());
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Close();
                    }
                }
            }
            if (!restoredSizes)
            {
                var writer = new FileStream("sizes.xml", FileMode.Create);
                Type type = windows.GetType();
                var serializer = new DataContractSerializer(type);
                serializer.WriteObject(writer, windows);
                writer.Close();
            }




            double[] sizes = new double[4];
            sizes[0] = window.Height;
            sizes[1] = window.Width;
            sizes[2] = window.Top;
            sizes[3] = window.Left;
            /*for (int i = 0; i < 4; i++)
            {
                System.Console.Write("size: {0}\n", sizes[i]);
            }*/
            if (windows.ContainsKey(window.Title))
            {
                windows.Remove(window.Title);
            }
            //System.Console.Write("Window.Title: {0}\n", window.Title);
            windows.Add(window.Title, sizes);
            //System.Console.Write("Window.Title: {0}\n", window.Title);

            windowValuesChanged(window);
            //window.SizeChanged -= window_SizeChanged;
            //window.SizeChanged += window_SizeChanged;
            //window.LocationChanged -= window_LocationChanged;
            //window.LocationChanged += window_LocationChanged;
        }

        static void window_LocationChanged(object sender, EventArgs e)
        {
            Window realSender = (Window)sender;
            windowValuesChanged(realSender);
        }

        static void windowValuesChanged(Window realSender)
        {
            //Rect rect = realSender.RestoreBounds;
            //System.Console.Write("r" + rect.ToString());
            if (windows.ContainsKey(realSender.Title))
            {
                //System.Console.WriteLine("c {4} : {0} - {1} - {2} - {3}\n", realSender.Height, realSender.Width, realSender.Top, realSender.Left, realSender.Title);
                //System.Console.Write("c Oldheight: {0} - {1}\n", windows[realSender.Title][0], realSender.Height);
                windows[realSender.Title][0] = realSender.Height;
                windows[realSender.Title][1] = realSender.Width;
                windows[realSender.Title][2] = realSender.Top;
                windows[realSender.Title][3] = realSender.Left;
                //save to File

                var writer = new FileStream("sizes.xml", FileMode.Create);
                Type type = windows.GetType();
                var serializer = new DataContractSerializer(type);
                serializer.WriteObject(writer, windows);
                writer.Close();
            }
        }

        static Dictionary<string, double[]> windows = new Dictionary<string, double[]>();

        static void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Window realSender = (Window) sender;
            windowValuesChanged(realSender);
        }
    }
}
