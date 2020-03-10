// This code is based on Gigi's Empty SDL2 window and handling keyboard and 
// mouse events tutorials:
// http://gigi.nullneuron.net/gigilabs/showing-an-empty-window-in-sdl2/
// http://gigi.nullneuron.net/gigilabs/handling-keyboard-and-mouse-events-in-sdl2/

#include <stdbool.h>
#include <SDL2/SDL.h>
 
int main(int argc, char ** argv)
{
    bool quit = false;
    SDL_Event event;
    int x = 288;
    int y = 208;
 
    SDL_Init(SDL_INIT_VIDEO);
 
    SDL_Window* window = SDL_CreateWindow("My SDL Empty Window",
        SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, 640, 480, 0);
    SDL_Renderer * renderer = SDL_CreateRenderer(window, -1, 0);

    SDL_Surface * image = SDL_LoadBMP("man.bmp");
    SDL_Texture * texture = SDL_CreateTextureFromSurface(renderer,
        image);
    SDL_FreeSurface(image);
 
    SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
 
    while (!quit)
    {
        SDL_WaitEvent(&event);
 
        switch (event.type)
        {
            case SDL_QUIT:
                quit = true;
                break;
        }
        SDL_Rect dstrect = { x, y, 64, 64 };
 
        SDL_RenderClear(renderer);
        SDL_RenderCopy(renderer, texture, NULL, &dstrect);
        SDL_RenderPresent(renderer);

    }
    SDL_DestroyTexture(texture);
    SDL_DestroyRenderer(renderer);
    SDL_DestroyWindow(window); 
    SDL_Quit();
 
    return 0;
}
