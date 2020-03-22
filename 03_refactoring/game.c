// This code is based on Gigi's Empty SDL2 window and handling keyboard and 
// mouse events tutorials:
// http://gigi.nullneuron.net/gigilabs/showing-an-empty-window-in-sdl2/
// http://gigi.nullneuron.net/gigilabs/handling-keyboard-and-mouse-events-in-sdl2/

#include <stdbool.h>
#include <SDL2/SDL.h>

typedef struct {
  SDL_Window * window;
  SDL_Renderer * renderer;
} Screen;

typedef struct {
  int x;
  int y;
} Player;

typedef struct {
  bool quit;
  SDL_Window * window;
  SDL_Renderer * renderer;
  SDL_Texture * texture;
  Player player;
} State;

SDL_Texture * loadGraphics(SDL_Renderer * renderer) {
  SDL_Surface * image = SDL_LoadBMP("man.bmp");
  SDL_Texture * texture = SDL_CreateTextureFromSurface(renderer, image);
  SDL_FreeSurface(image);
  return texture;
}

void handleEvents(State * state) {
  SDL_Event event;
  SDL_WaitEvent(&event);
  switch (event.type)
  {
    case SDL_QUIT:
      state->quit = true;
      break;
  }
}

void drawGraphics(State * state) {
    SDL_Rect dstrect = { state->player.x, state->player.y, 64, 64 };
    SDL_RenderClear(state->renderer);
    SDL_RenderCopy(state->renderer, state->texture, NULL, &dstrect);
    SDL_RenderPresent(state->renderer);
}

State initState() {
  SDL_Window * window = SDL_CreateWindow(
      "My SDL Empty Window",
      SDL_WINDOWPOS_UNDEFINED,
      SDL_WINDOWPOS_UNDEFINED,
      640,
      480,
      0);
  SDL_Renderer * renderer = SDL_CreateRenderer(window, -1, 0);
  State state = {
    .quit = false,
    .renderer = renderer,
    .window = window,
    .texture = loadGraphics(renderer),
    .player = {
      .x = 288,
      .y = 208,
    }
  };
  return state;
}

int cleanUpAndExit(State * state) {
  SDL_DestroyTexture(state->texture);
  SDL_DestroyRenderer(state->renderer);
  SDL_DestroyWindow(state->window); 
  SDL_Quit();
  return 0;
}

int main(int argc, char ** argv)
{
  SDL_Init(SDL_INIT_VIDEO);
  State state = initState();

  SDL_SetRenderDrawColor(state.renderer, 255, 255, 255, 255);

  while (!state.quit)
  {
    handleEvents(&state);
    drawGraphics(&state);
  }
  return cleanUpAndExit(&state);
}
