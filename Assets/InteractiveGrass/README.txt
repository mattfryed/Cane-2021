Thanks for download asset. You can write in alexsampublic@outlook.com for questions.

## WHAT'S IN THE ASSET ? 
---------------------------------------------------------------------
The asset has interactive grass. Grass reacts to objects that move on it. Adjust the wind that will shake the grass.

---------------------------------------------------------------------
## HOW TO WORK WITH ASSET?
---------------------------------------------------------------------
- Open DemoScene
- You can set the speed in the movement component on the Sphere object (speed parameter).
- Set up the grass. Select the Grass object and set the parameters in the Grass script.
-- Wind Direction - the direction in which the wind is moving.
-- Wind Strength - the strength with which the wind affects the grass.
-- Grass Count - Grass density. The higher this value, the more grass will be displayed.
-- Grass Mesh - mesh for show grass. Use quad as default.
-- Grass Material - a material that contains a shader for displaying grass.
-- Grass Quad Size - quad scale for grass.
-- Targets - sources, which interacts with the grass and presses on it. Targets limits = 100. 
If you want to increase the limit - in Grass.shader find the line "float4 _PlayerPositions[100];" and change the value 100 to something else. 
You can also decrease this value for optimization.
-- Offset Radius - the radius of the trampled grass around the object.
-- Strength - grass crushing strength.
-- Bounds Radius - border size.
- You can also use this logic in your scenes. Create a plane, add the Grass script, and fill in the fields.
- You can customize the material. Find the Grass material in the Materials folder.
-- Texture - you can add grass texture to be displayed. Several alternative textures are in the Textures folder.
-- Color - grass texture color
-- Alpha Cutoff - option to cut off unnecessary details.
-- Wind Direction - the direction in which the wind is moving.
-- Noise map - texture for more interesting wind simulation. You can use different noise textures, find the appropriate textures in the Textures folder.
-- Wind Strength - the strength with which the wind affects the grass.

If you see that the grass is no longer displayed, resetting the settings will help you.
To do this, right-click on the Grass script (located in your land) and call UpdateBuffers from the context menu.


