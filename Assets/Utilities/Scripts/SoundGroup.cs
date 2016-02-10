//SoundGroup by Daniel Snd (http://snddev.tumblr.com/utilities) is licensed under:
/* The MIT License (MIT)
Copyright (c) 2013 UnityPatterns
Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the “Software”), to deal in the
Software without restriction, including without limitation the rights to use, copy,
modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
and to permit persons to whom the Software is furnished to do so, subject to the
following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. */

using UnityEngine;
using System.Collections;

/// <summary>
/// Soundgroup is used in conjunction with SoundManager.
/// It stores audioclips and can pick a random audioclip
/// from an array to play. This audioclip can use a random
/// pitch to add further variation to the sounds in the game.
/// 
/// In the case of music, it can play audioclips one after
/// the other like a playlist.
/// </summary>
[RequireComponent (typeof (AudioSource))]
public class SoundGroup : MonoBehaviour {

	public AudioClip[] Sounds;
	public bool Music=false;
	public bool RandomPitch=false;
    public float RandomPitchMin = 0.9f;
    public float RandomPitchMax = 1.1f;
	private float startingvolume;
	private int played=0;
    [HideInInspector]
    public string identifier;
    [HideInInspector]
    public AudioSource mAudio;
	
	void Awake() {
        mAudio = GetComponent<AudioSource>();

        //Save the volume set on the inspector as our starting volume.
        startingvolume = mAudio.volume;

        //if it's music let's make sure it's not destroyed on load, so
        //it persists on level changes.
        if(Music)
            DontDestroyOnLoad(this.gameObject);
	}
	
    /// <summary>
    /// Since we're using a pooling system, when we get out of the pool OnEnable
    /// will be called by unity.
    /// </summary>
	void OnEnable () {
        //If there isn't any sounds in our array we can't play anything, return this to the pool.
		if(Sounds.Length==0) {
			Debug.Log("There is no sound on "+gameObject.name+" SoundGroup, ABORT");
		    SoundManager.RecycleSoundToPool(this);
			return;
		}

        //If there isn't an audio reference for some reason, attempt to get one.
		if(!mAudio)
			gameObject.AddComponent<AudioSource>();

        //Pick and assign an audio clip.
		PickAudio();

        //If we're using the random pitch, pick and assign one.
		if(RandomPitch)
			mAudio.pitch = Random.Range(RandomPitchMin,RandomPitchMax);

        //Set the volume based on the current volume value of the SoundManager.
		SetVolume();

        //Play audio source.
		mAudio.Play();
        
        //Start a coroutine to wait until we stop playing the audio source.
		StartCoroutine(WaitForStopPlaying());
	}

    /// <summary>
    /// Set the volume based on the current volume value of the SoundManager.
    /// Also taking into consideration the initial volume we set in the inspector
    /// for this specific prefab.
    /// </summary>
    public void SetVolume() {
		if(Music)
			mAudio.volume=SoundManager.instance.MusicVolume*startingvolume;
		else
			mAudio.volume=SoundManager.instance.SFXVolume*startingvolume;
	}
	
    /// <summary>
    /// Pick an audioclip to play.
    /// </summary>
	void PickAudio() {
        //If this prefab is a music soundgroup
		if(Music) {
            //Pick the last audioclip played. Default is 0
			mAudio.clip = Sounds[played];

            //If this isn't the last audioclip in the array, add 1 to played value,
            //so we can play next music in the playlist after this one. Otherwise
            //Set it to 0 so we can replay the first music of the play list after this.
			if(played<Sounds.Length-1)
				played++;
			else
				played=0;
		} else {
            //This isn't a music soundgroup, so it's a SFX one. Just pick a random one
            //from the array and assign it.
			mAudio.clip = Sounds[Random.Range(0, Sounds.Length)];
		}
	}
	
    /// <summary>
    /// Force the audio to stop playing and recycle it.
    /// </summary>
	public void ForceStop() {
        //Stop the coroutine that watches for the audio to finish playing
		StopAllCoroutines();
        //Recycle the audio to the pool.
        SoundManager.RecycleSoundToPool(this);
    }

    /// <summary>
    /// When the game is alt+tabbed or loses focus unity calls this function.
    /// </summary>
    /// <param name="focus"></param>
    void OnApplicationFocus(bool focus)
    {
        if (!Application.isEditor)
        {
            //If we're in a build.
            if (!focus)
            {
                //The window lost focus we should stop the coroutine that watches
                //for the sound to stop playing and pause the audio listener.
                StopAllCoroutines();
                AudioListener.pause = true;
            }
            else
            {
                //The window gained focus, we should unpause the audio listener
                //so we can keep on playing the sound from where we stopped.
                //And restart the coroutine that will wait for the sound to
                //stop playing.
                AudioListener.pause = false;
                StartCoroutine(WaitForStopPlaying());
            }
        }
        else
        {
            //Don't pause if on editor, because on editor we lose focus all the time.
            AudioListener.pause = false;
            StartCoroutine(WaitForStopPlaying());
        }
    }

    /// <summary>
    /// This coroutine waits until the sound stops playing to recycle it or
    /// in the case of Music Soundgroup, go to the next music in the list.
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitForStopPlaying()
    {
        //While audio is playing wait for 0.05f seconds
        //and then check again if audio is still playing.
        while (mAudio.isPlaying)
        {
            yield return new WaitForSeconds(0.05f);
        }

        //Audio isn't playing anymore if we got to this point.
        yield return null;
        
        //if it's music and we're still active, just call
        //OnEnable again to play next music. Otherwise just
        //recycle it back to the pool.
        if(SoundManager.instance.currentMusic==this&&enabled)
        	OnEnable();
        else
            SoundManager.RecycleSoundToPool(this);
    }
}
