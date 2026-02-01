namespace AIRPG.Core.Navigation;

public interface INavigationService
{
    public void ToMainMenu();
    public void ToCampaignMenu();
    public void ToSettings();
    public void ToGallery();
    public void ToBrewery();
    public void Exit();
}
