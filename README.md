# Final Project

https://github.com/user-attachments/assets/f12309df-7907-4f5f-b200-3cf60652c8f7

Downloadable demo executables available at https://github.com/jyguan18/final-project-griffin-jackie/releases/tag/Demos

## Key Features
- [X] Integrated, Infinite Generation: The Wave Function Collapse algorithm, used to place city structures (castles and houses), directly interacts with the height data from the Perlin noise-based terrain. Tiles are selected based on constraints for specific height ranges and slopes, which makes them appear logically in the landscape.
- [X] Adaptive Geometry: The WFC-generated models adapt their vertices to the underlying terrain height. This allows structures like castle walls to hug the landscape while still retaining the integrity of vertical structures like towers by selectively applying height adaptation.
- [X] Aesthetic Stylization: A custom post-process shader was implemented to achieve the distinctive watercolor aesthetic, utilizing techiques like color quantization, edge darkening, and a paper-like overlay.
- [X] Exploration: The scene is fully traversible with a first-person player view, allowing users to explore the infinite, chunk-loaded terrain and the procedurally placed architecture.

## Final Output
Below are images demonstrating the final, stylized and explorable environment.

## Post-Mortem
Overall, the project was a success and we accomplished everything that we were initially set out to accomplish. This was largely due to our evaluation of the project's scope in advance—in our initial concept, we considered a form of user-editable WFC, in the vein of how Townscaper uses user input to determine which tiles to fill with buildings vsersus blank spaces, but we decided that a less-interactive experience would be more manageable in scope (not requiring a GUI to be integrated into it and having less unpredictable elements), while we could still capture some user-interaction by allowing the user to explore the generated world in first person. 

#### Goal Achievement
- [X] Height-based terrain with height-based gradient
- [X] Wave Function Collapse to populate the terrain with buildings
- [X] First-Person Exploration
- [X] Stylization Post-Process with a Water Shader  

#### Pivots and Challenges

For the post-process effect, I decided to move to the Universal Render Pipeline (URP), similar to our homework. The initial setup took a while to do to include it into our current scene and I had to remake a few shaders (to become shadergraphs) to work for URP. In addition, the move to the URP conflicted with the water shader implementation, which prevented the water from rendering correctly with transparency. We initially tried to set the water material to opaque, which showed up but caused some (very obvious) visual artifacts. We ended up implementinga custom URP pass that captured the scene, including the transparent water elements, and passed the result as a scene texture to the post-process volume. This ensured the transparent components were correctly sampled and processed by the subsequent post-process effects, resolving the conflict.

Developing the watercolor post-process shader was an iterative and challenging process. I had to think more creatively about how I wanted to do it, since I wasn't strictly following a tutorial. The solution ended up involving breaking the shader logic into modular subgraphs. This allowed me to test each subgraph to make sure it worked, easier to tweak parameters, in order to achieve the desired style.

# Milestone 1

## Wave Function Collapse

Implemented a basic Wave Function Collapse algorithm in Unity, with a flat 2D map. Two main types of components are used: tiles, which store lists of possible adjacent tiles, and the generator itself, which has parameters for the map dimensions, grid cell size, and a list of possible tile types to appear in the map. Currently tiles only check orthogonally-adjacent tiles (i.e. tiles within the von Neumann neighborhood of that tile) but this could be easily expanded (e.g. to a Moore neighborhood) depending on the needs of the tileset by adding additional arrays to the tile class and additional steps to the propogation function used in the generator. Another potential feature which is not currently included is varied tile weights, as each possible tile currently has an equal chance of being selected in a given step (though their effective frequency of appearance varies depending on how their connections occur within the grid).

<img width="410" height="537" alt="Screen Shot 2025-11-13 at 12 01 07 AM" src="https://github.com/user-attachments/assets/f034fcfc-8b3c-4e49-9c23-c1d2bcbe4ed6" />
<img width="410" height="539" alt="Screen Shot 2025-11-13 at 12 00 06 AM" src="https://github.com/user-attachments/assets/05a71e4a-be3f-497d-934d-6f1c8fb6e0a0" />
<img width="410" height="535" alt="Screen Shot 2025-11-12 at 11 58 54 PM" src="https://github.com/user-attachments/assets/76926983-0907-4d84-a17c-bca175f9bfff" />
<img width="410" height="537" alt="Screen Shot 2025-11-13 at 12 00 43 AM" src="https://github.com/user-attachments/assets/4d70ce51-203f-414f-a9c1-b1f3c28627ee" />

The above images show example outputs using a set of castle parts made in Maya. 

<img width="992" height="762" alt="Screen Shot 2025-11-13 at 12 05 43 AM" src="https://github.com/user-attachments/assets/5e7e9bc3-da3b-47ce-bf04-08c48cb68e39" />

<img width="1178" height="641" alt="Screen Shot 2025-11-13 at 12 00 20 AM" src="https://github.com/user-attachments/assets/16c3ad75-1788-4c98-ad0b-e79f17c0c59d" />


## Terrain Generation

Implemented basic Perlin-noise based terrain generation. I followed [this tutorial](https://www.youtube.com/watch?v=hNRFosb_3Tc) to set up the base of the terrain generation and it worked really well. To add onto it, I implemented chunking for infinite terrain. So, I added a basic player capsule and depending on the location of the player, chunks will be spawned (or destroyed) within a certain distance. As part of the tutorial I followed, I also added a basic gradient shader that colors the terrain based on height.

![Untitledvideo-MadewithClipchamp8-ezgif com-video-to-gif-converter](https://github.com/user-attachments/assets/30da67fa-9746-44f6-9dee-a9ce3c5dadc4)
![Recording2025-11-12232842-ezgif com-video-to-gif-converter](https://github.com/user-attachments/assets/93e1ca3f-a6f2-4d7b-8691-ee0b569e2c1d)

# Milestone 2

## Updating and Combining WFC with Terrain

The Wave Function Collapse-based city generation algorithm connects to the varied height of the terrain in two ways. WFC is still performed using a 2D grid of cells, but as we construct the grid we sample the height of the terrain for a grid of positions (using the X and Z coordinates of the corners, centers of each edge, and cell center) under each cell in order to get a minimum and maximum height for the terrain under that cell. Each cell then has the set of possible tiles for that cell updated based on a set of parameters configurable per each tile type: a set of allowed height bounds, where the cell must have its minimum and maximum lie within those bounds in order to use this type of tile, and a maximum height difference, where if the difference in height that occurs within that cell is greater than the maximum then that tile also cannot be used in this cell. After updating these possibilities, the changes are propagated just as when we select a cell's tile type in the main algorithm.

<img width="1339" height="827" alt="Screen Shot 2025-11-24 at 11 42 01 PM" src="https://github.com/user-attachments/assets/c6352b17-eaf7-4245-bad4-8cad0feefd48" />

For example, the above image shows a scene generated from a set of tiles that are split into two groups: one is the light-colored castle tiles, while the other is the dark-colored house tiles. Both tiles have their minimum height bound set so that they cannot appear below a certain value representing the water level, so that they cannot appear in any cell which has a height sample lying below the water level. The castle tiles have a slightly higher minimum than the house tiles, so the houses appear slightly more frequently in the low-lying areas as only they can appear at the lowest points above water. The house tiles however have a lower max height difference value than the castle tiles, meaning that the castle tiles can appear on steeper slopes than the house tiles, such as on the hills in the center and upper left of the above image.

The other form of connection between the systems is done in the creation of the models themselves after the tile types are chosen. The vertices of the model are all transformed based on the height of the terrain directly under that particular vertex. Components of the tile objects can configured to behave in two different ways: either all of the vertices map directly with the terrain height under them, or only vertices at or below a configurable ground level use the terrain directly under them, while the rest of the vertices use a center height shared across the whole tile. This allows features like the castle walls to fit closely to the landscape using the first configuration, enabling smooth connections between the upper walkways, while structures like the upper parts of towers don't slope and instead are simply raised evenly in order to not appear lopsided.

<img width="1339" height="835" alt="Screen Shot 2025-11-24 at 11 42 58 PM" src="https://github.com/user-attachments/assets/f5c4e976-837c-4344-a64a-eb6cd198705b" />

The configuration of tile types was also reworked and made simpler to modify than in the first milestone, with each tile being able to automatically generate rotated counterparts using a set of parameters controllable on the Unity side, as well as being able to use a set of flags to group together sets of tiles that are all valid neighbors in the same situations. For example, all of the castle wall tiles share the "Wall" flag, such that each wall can reference its valid neighbors for each direction it should connect to as simply being any tile which has that "Wall" flag. This facilitates the easier creation of a larger set of tiles, such as the house tiles used in the above examples.

## Water Shader
I created a water shader, which is really just a plane to show the water. I did run into some issues with the render pipeline, and it does not show with the current post-process step.

![ScreenRecording2025-11-24013328-ezgif com-video-to-gif-converter](https://github.com/user-attachments/assets/1758e9e1-ad7b-4558-8abd-64c2bf0cd648)

## Post-Process Shader
For the post-process shader, I tried to mimic a watercolor, cute style. There weren't a lot of good resources to reference, but I loosely looked at [this example](https://www.bruteforcegame.com/post/watercolor-shader-devblog-13). Of course, all the work is different since this is an older blog. I basically split the post-process shadergraph into 4 subgraphs: PaperOverlay, EdgeDarkening, WaterBloom, and ColorQuantization. The goal was to bucket the colors in the scene and apply some effects to make it look more watercolory.

<img width="1366" height="781" alt="image" src="https://github.com/user-attachments/assets/831bb455-ee90-43c0-81ad-3e00ab924444" />

## First Person Player View
We also added a first person player view. This allows the player to move around in the terrain and get a better look at the environment.

![ScreenRecording2025-11-24173744-ezgif com-video-to-gif-converter (2)](https://github.com/user-attachments/assets/d341ab16-3485-4889-8256-95d0bb902f45)

# Design Document

## Introduction
We were inspired by the game Townscaper to create a procedurally generated, stylized environment where a player can explore and walk around in.

## Goal
The goal of this project is to procedurally generate a beautiful, explorable environment that combines algorithmic generation with aesthetic design. We want to use procedural rules to not only build geometry, but also be visually appealing and cohesive.

Specifically, we will:
- Generate a stylized terrain with water and elevation variation
- Populate it with procedurally generated buildings using wave function collapse 
- Allow the player to explore the scene in first person

## Inspiration/Reference
- Townscaper
<img width="1005" height="1007" alt="Screen Shot 2025-03-23 at 5 16 27 PM" src="https://github.com/user-attachments/assets/9d601148-04c8-40ec-992c-75c783f7448d" />
- [Infinite procedurally generated city with the Wave Function Collapse algorithm | Marian's Blog](https://marian42.de/article/wfc/)


## Specification
- Engine: Unity
- Camera: First-person exploration
- Programming Languages: C#
- Features:
  - Procedural terrain and water generation
  - Procedural building generation
  - Stylization
  - First-Person Controls

## Techniques
- Perlin Noise
- Wave Function Collapse
  - https://www.boristhebrave.com/2020/04/13/wave-function-collapse-explained/
  - https://www.boristhebrave.com/2021/06/06/driven-wavefunctioncollapse/
  - https://www.boristhebrave.com/2021/10/26/model-synthesis-and-modifying-in-blocks/ 
  - https://www.boristhebrave.com/2021/10/31/constraint-based-tile-generators/ 
- Stylization
  - https://github.com/jyguan18/hw02-stylization

## Design
<img width="1379" height="755" alt="Screen Shot 2025-11-05 at 2 52 37 PM" src="https://github.com/user-attachments/assets/089118cc-045f-4f36-b161-b7bf1e022c20" />


## Timeline

| |Griffin|Jackie|Deadline|
|--|--|--|--|
|**Milestone 1**|- Gather assets<br>- Wave Function Collapse for a flat plane|- Setup Github<br>- Gather assets<br>- Terrain Generation|Nov 12, 2025|
|**Milestone 2**|- First-Person Camera<br>- Adapt wave function collapse to the generated terrain|- Stylization (Post-Process Effects)<br>- Water Shading<br> - First-Person Camera|Nov 24, 2025|
|**Final**|- Final Tweaks<br>- Tune movement + collision|- Final Tweaks<br>- Tune colors, textures, etc.|Dec 1, 2025|
