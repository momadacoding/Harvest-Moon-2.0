using Godot;

public partial class GameSoundManager : Node2D
{
    public Godot.Collections.Dictionary<AudioStreamPlayer, double> sound_dictionary { get; } = new();

    public void play_effect(string sound_to_play)
    {
        PlayInGroup("Effects", sound_to_play);
    }

    public void play_music(string sound_to_play)
    {
        PlayInGroup("Music", sound_to_play);
    }

    public void play_tool(string sound_to_play)
    {
        PlayInGroup("Tools", sound_to_play);
    }

    public void stop_music(string sound_to_stop)
    {
        foreach (var music in GetNode<Node>("Music").GetChildren())
        {
            if (music is AudioStreamPlayer player && player.Name == sound_to_stop)
            {
                player.Stop();
            }
        }
    }

    public void set_music_volume(string sound_to_set, float amount)
    {
        foreach (var music in GetNode<Node>("Music").GetChildren())
        {
            if (music is AudioStreamPlayer player && player.Name == sound_to_set)
            {
                player.VolumeDb = amount;
            }
        }
    }

    public bool is_playing(string sound_to_check)
    {
        foreach (var group in GetChildren())
        {
            foreach (var sound in group.GetChildren())
            {
                if (sound is AudioStreamPlayer player && player.Name == sound_to_check)
                {
                    return player.Playing;
                }
            }
        }

        return false;
    }

    public void pause_all_sounds()
    {
        sound_dictionary.Clear();

        foreach (var group in GetChildren())
        {
            foreach (var sound in group.GetChildren())
            {
                if (sound is AudioStreamPlayer player && player.Playing)
                {
                    sound_dictionary[player] = player.GetPlaybackPosition();
                    player.Stop();
                }
            }
        }
    }

    public void resume_all_sounds()
    {
        foreach (var soundPlayer in sound_dictionary.Keys)
        {
            soundPlayer.Play((float)sound_dictionary[soundPlayer]);
        }

        sound_dictionary.Clear();
    }

    private void PlayInGroup(string groupName, string soundName)
    {
        foreach (var sound in GetNode<Node>(groupName).GetChildren())
        {
            if (sound is AudioStreamPlayer player && player.Name == soundName)
            {
                player.Play();
            }
        }
    }
}
