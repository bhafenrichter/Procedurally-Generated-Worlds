# Procedurally-Generated-Worlds

In the aftermath of COVID-19, I wasn't happy to be stranded in my small studio apartment in Berlin.  Therefore, I decided to create a procedurally generated world so I could at least emulate the outside world while the world governments figure this thing out.  To do so, I used the following tools:

### Unity 

A very flexible and versatile game engine with plenty of documentation to tackle such a task.  

###  The World

I used a variation of Perlin Noise to generate the first iteration of the world.  This resulted in good results, but ultimately the randomness became repetitive.  You could expect the kind of world you were going to get.  Therefore, I needed a different technique.  Using Simplex Noise, I created much more organic noise that lead to worlds with a larger variety of land mass sizes.

###  Nature

For nature, I wanted to create various biomes including forests and praries.  An element common to both of these is the frequency at which foliage and trees appear.  Forests obviously having much more in contrast to praries.  For this section, I decided to use the Poisson Disk Sampling technique to group nature elements together and to make sure that the models were decently spaced.  One issue I ran into in the first iteration, was a completely random location placement looks artificial and fake.  Poisson Disk Sampling saved me here.

### Lakes

Where there were large depressions in the generated land mesh, I wanted to place a lake.  This would be incredibly simple if I just had one prop mesh that expanded over the entire map at a certain threshold, but I wanted to be a bit more pragmatic.  This is where the K-Means Algorithm came in.  Using this algorithm, I could group together points that were all clustered together that met the threshold for being a lake.  Once I gathered the various clusters, I could compute the size of the various meshes dynamically and manage each lake on its own.

### Next Steps

After all of this procedural generation, I was one step closer to making a better "New Reality" than the one Twitter keeps telling me we'll have.  Unfortunately, its still got a ways to go.  Next steps would include:

1.  Adding Rivers (in development)
2.  Create Dynamic Villages
3.  NPCs
4.  Nature Shaders for Water and Foliage
