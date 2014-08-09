using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KinectAttract
{
	/// <summary>
	/// Interaction logic for Attractatron.xaml
	/// </summary>
	public partial class Attractatron : UserControl
	{
	  public void LookForUser()
	  {
      // Search around looking for someone to talk to...
	    Eyes.LookAround();
	  }

	  public void FoundUser()
	  {
      // "Hey! Hey, you!"
	  }

	  public void GainInterest()
	  {
      // Dilate pupils, we're interested...
	    Eyes.FoundPerson();
	  }

	  public void LooseInterest()
	  {
      // Meh, they've wandered off...
	    Eyes.LostPerson();
	  }

		public Attractatron()
		{
			this.InitializeComponent();
		}
	}
}