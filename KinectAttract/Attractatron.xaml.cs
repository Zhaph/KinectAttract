using System.Windows.Controls;

namespace KinectAttract
{
  /// <summary>
  ///   Interaction logic for Attractatron.xaml
  /// </summary>
  public partial class Attractatron : UserControl
  {
    public Attractatron()
    {
      InitializeComponent();
    }

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
  }
}