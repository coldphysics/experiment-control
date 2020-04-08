using Model.Settings;
using Model.Settings.Settings;
using System;

/// <summary>
/// Contains a bunch of quick accessors to commonly used settings that are available for any kind of experiment
/// </summary>
public static class Global
{
    #region Only kept for model-backwards compatibility
    /// <summary>
    /// An enumeration type containing the names of all the experiments which use this software.
    /// </summary>
    public enum ExperimentTypes
    {
        Undefined,
        AdWin5thFloor,
        Superatom,
        ColdRydberg,
        Dy4thFloor,
        AdWinSimulator
    }

    /// <summary>
    /// The name of the experiment, set initially to (Undefined)
    /// </summary>
    public static ExperimentTypes Experiment = ExperimentTypes.Undefined;

    /// <summary>
    /// Total number of buffers used in the experiment
    /// </summary>
    public static uint NumberOfBuffers;

    #endregion


    public static HW_TYPES GetHardwareType()
    {
        ProfilesManager profilesManager = ProfilesManager.GetInstance();
        Profile profile = profilesManager.ActiveProfile;
        string hwTypeString = ((StringMultiOptionSetting)profile.GetSettingByName(SettingNames.HW_TYPE)).Value;
        HW_TYPES type = (HW_TYPES)Enum.Parse(typeof(HW_TYPES), hwTypeString, true);

        return type;
    }

    public static bool CanAccessDatabase()
    {
        ProfilesManager profilesManager = ProfilesManager.GetInstance();
        Profile profile = profilesManager.ActiveProfile;

        bool result = ((BooleanSetting)profile.GetSettingByName(SettingNames.ALLOW_ACCESS_DATABASE)).Value;

        return result;
    }

    public static int GetNumDigitalCards()
    {
        ProfilesManager profilesManager = ProfilesManager.GetInstance();
        Profile profile = profilesManager.ActiveProfile;   
        int numOfDigitalCards = ((IntegerSetting)profile.GetSettingByName(SettingNames.NUM_DIGITAL_CARDS)).Value;

        return numOfDigitalCards;
    }

    public static int GetNumAnalogCards()
    {
        ProfilesManager profilesManager = ProfilesManager.GetInstance();
        Profile profile = profilesManager.ActiveProfile;
        int numOfAnalogCards = ((IntegerSetting)profile.GetSettingByName(SettingNames.NUM_ANALOG_CARDS)).Value;

        return numOfAnalogCards;
    }

    public static int GetNumDigitalChannelsPerCard()
    {
        ProfilesManager profilesManager = ProfilesManager.GetInstance();
        Profile profile = profilesManager.ActiveProfile;
        int numOfDigitalChannels = ((IntegerSetting)profile.GetSettingByName(SettingNames.NUM_DIGITAL_CHANNELS_PER_CARD)).Value;

        return numOfDigitalChannels;
    }

    public static int GetNumAnalogChannelsPerCard()
    {
        ProfilesManager profilesManager = ProfilesManager.GetInstance();
        Profile profile = profilesManager.ActiveProfile;
        int numOfAnalogChannels = ((IntegerSetting)profile.GetSettingByName(SettingNames.NUM_ANALOG_CHANNELS_PER_CARD)).Value;

        return numOfAnalogChannels;
    }

    public static int GetNumOfBuffers()
    {
        ProfilesManager profilesManager = ProfilesManager.GetInstance();
        Profile profile = profilesManager.ActiveProfile;
        int numOfAnalogCards = ((IntegerSetting)profile.GetSettingByName(SettingNames.NUM_ANALOG_CARDS)).Value;
        int numOfDigitalCards = ((IntegerSetting)profile.GetSettingByName(SettingNames.NUM_DIGITAL_CARDS)).Value;
        int numOfAnalogChannels = ((IntegerSetting)profile.GetSettingByName(SettingNames.NUM_ANALOG_CHANNELS_PER_CARD)).Value;
        int numBuffers = numOfAnalogCards * numOfAnalogChannels + numOfDigitalCards;

        return numBuffers;
    }

}
