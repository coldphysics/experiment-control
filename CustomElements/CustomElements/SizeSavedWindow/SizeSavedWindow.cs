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
        // To keep a window at the same size and position, just add 
        // SizeSavedWindow.addToSizeSavedWindows(this);
        // right after initialieComponent
        public static void addToSizeSavedWindows(Window window)
        {
            window_IsLoaded(window, null);
            window.Closed += window_Closing;
        }

        static void window_Closing(object sender, EventArgs e)
        {             
            Window realSender = (Window)sender;
            windowValuesChanged(realSender);
        }

        static void window_IsLoaded(object sender, RoutedEventArgs e)
        {
            Window window = (Window)sender;

            bool restoredSizes = false;

            if (File.Exists("sizes.xml"))
            {
                var stream = new FileStream("sizes.xml", FileMode.Open);

                try
                {
                    var reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas());
                    var deserializer = new DataContractSerializer(typeof(Dictionary<string, double[]>));
                    windows = (Dictionary<string, double[]>)deserializer.ReadObject(reader, true);

                    foreach (KeyValuePair<string, double[]> pair in windows)
                    {
                        if (pair.Key == window.Title)
                        {
                            
                            //THIS WINDOW IS ALREADY IN THE FILE
                            window.Height = pair.Value[0];
                            window.Width = pair.Value[1];
                            window.Top = pair.Value[2];
                            window.Left = pair.Value[3];
                            break;
                        }
                        
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

            if (windows.ContainsKey(window.Title))
            {
                windows.Remove(window.Title);
            }

            windows.Add(window.Title, sizes);
            windowValuesChanged(window);
        }

        static void window_LocationChanged(object sender, EventArgs e)
        {
            Window realSender = (Window)sender;
            windowValuesChanged(realSender);
        }

        static void windowValuesChanged(Window realSender)
        {
            if (windows.ContainsKey(realSender.Title))
            {
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
