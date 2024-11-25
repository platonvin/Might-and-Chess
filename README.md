my first attempt on using unity game engine

##### Some footage
https://github.com/user-attachments/assets/11287429-2847-4bd0-b12f-cc0ae27d95c4

game is ~2k lines total (smaller than headers files for my renderer)
I am not sure why, but even simplest unity project i've seen are tens and hundreds of files for something i would not even call a game

please ignore the codestyle 
whole game is a single monobeh class (which is considered bad practice (by those who create 100kloc clickers that do not even work and lag on 4090))
c# features are sometimes ignored or misused (c# problem, not mine)

i would do same in cpp in like 0.2 of time spent (~12 hours) and also in pure (lum-al) Vulkan and build would be ~1.2 mb. Experiment is over - i will not use Unity for any of my games. Kinda enjoyed it tho

If you have a lot of free time, you can try to improve and simplify the whole thing.

art is stolen from greatest strategy game ever (homm3) (took ~6 hours to extract it from game lol)
things are rendered via instantiation. No gpu-side programming, just "animated" sprites + little bit of inverse kinematics

the moral of the story - ignore i-do-everything-engines, they are not useful yet
