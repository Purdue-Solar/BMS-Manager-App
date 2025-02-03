using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMSManagerRebuilt.GUIOperation
{
    class ButtonOperation
    {
        /// <summary>
        /// Configure Button Operation(Interrupts)
        /// </summary>
        /// <param name = "sender" > Will contained data about the XAML processor that triggered it</param>
        /// <param name = "e" ></ param >
        public static void ConfigureButton_Click(object sender, string text)
        {
            ChangeText((TextBox)sender, text);
        }

        /// <summary>
        /// Transition Button Operation (Interrupts)
        /// </summary>
        /// <param name="sender">Will contained data about the XAML processor that triggered it</param>
        /// <param name="e"></param>
        public static void TransitionButton_Click(object sender, RoutedEventArgs e)
        {

        }

        public static void ChangeText(TextBox myBox, string text)
        {
            myBox.Text = text;
        }


    }
}
