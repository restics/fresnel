# Fresnel: Stereoscopic 3d XR client made in Unity

## What does this do??
 - this takes a side by side stereoscopic livestream and displays it in stereoscopic 3d in Meta Quest headsets
 - designed for ultra low latency use cases as livekit uses webrtc for both ingress and egress

## Installation
 - this project was developed on Unity 6000.1.4f1, Livekit Unity SDK 1.2.3 and Meta XR AIO Unity SDK v74.0.0
 - git clone and import project, then open in Unity Hub
 - connect your headset to meta quest link if you are using that, otherwise you can try it out with just the Meta XR simulator included in the AIO SDK
 - press run to test in editor
 - if you want to build the application into your headset as an apk:
     - first set up a build profile in file > build profiles > android > run device
     - select the device you want to build to
     - then go to meta > ovr build > ovr build apk (this basically just reuses gradle caches)



todo: finish readme

