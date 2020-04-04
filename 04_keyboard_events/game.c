// This code is based on Gigi's Empty SDL2 window and handling keyboard and 
// mouse events tutorials:
// http://gigi.nullneuron.net/gigilabs/showing-an-empty-window-in-sdl2/
// http://gigi.nullneuron.net/gigilabs/handling-keyboard-and-mouse-events-in-sdl2/

#include <stdbool.h>
#include <time.h>
#include <SDL2/SDL.h>

typedef struct {
  SDL_Window * window;
  SDL_Renderer * renderer;
  SDL_Texture * texture;
} Graphics;

typedef struct {
  float x;
  float y;
  float vx;
  float vy;
  float ax;
  float ay;
  float f;
} Player;

typedef struct {
  double dt;
  time_t t;
  double acc;
} Time;

time_t currentTime() {
  struct timespec ts;
  clock_gettime(CLOCK_REALTIME, &ts); 
  return ts.tv_nsec;
}

SDL_Texture * loadGraphics(SDL_Renderer * renderer) {
  SDL_Surface * image = SDL_LoadBMP("man.bmp");
  SDL_Texture * texture = SDL_CreateTextureFromSurface(renderer, image);
  SDL_FreeSurface(image);
  return texture;
}

Graphics initGraphics() {
  SDL_Window * window = SDL_CreateWindow(
      "Game",
      SDL_WINDOWPOS_UNDEFINED,
      SDL_WINDOWPOS_UNDEFINED,
      640,
      480,
      0);
  SDL_Renderer * renderer = SDL_CreateRenderer(window, -1, 0);
  SDL_Texture * texture = loadGraphics(renderer);
  Graphics graphics = {
    .window = window,
    .renderer = renderer,
    .texture = texture
  };
  return graphics;
}

Player initPlayer() {
  Player player = {
    .x = 288,
    .y = 208,
    .vx = 0,
    .vy = 0,
    .ax = 0,
    .ay = 0,
    .f = 1
  };
  return player;
}

Time initTime() {
  Time time = {
    .dt = 0.01,
    .t = currentTime(),
    .acc = 0
  };
  return time;
}

void handleKeyboard(Player * player) {
  const Uint8 *keyboardState = SDL_GetKeyboardState(NULL);
  if (keyboardState[SDL_SCANCODE_A] && keyboardState[SDL_SCANCODE_D]) {
    player->ax = 0;
  } else if (keyboardState[SDL_SCANCODE_A]) {
    player->ax = -player->f;
  } else if (keyboardState[SDL_SCANCODE_D]) {
    player->ax = player->f;
  } else {
    player->ax = 0;
  }
  if (keyboardState[SDL_SCANCODE_W] && keyboardState[SDL_SCANCODE_S]) {
    player->ay = 0;
  } else if (keyboardState[SDL_SCANCODE_W]) {
    player->ay = -player->f;
  } else if (keyboardState[SDL_SCANCODE_S]) {
    player->ay = player->f;
  } else {
    player->ay = 0;
  }
}

void handleEvents(Player * player, bool * quit) {
  SDL_Event event;
  SDL_PollEvent(&event);
  switch (event.type)
  {
    case SDL_QUIT:
      *quit = true;
      break;
  }
  handleKeyboard(player);
}

int cleanUpAndExit(Graphics * graphics) {
  SDL_DestroyTexture(graphics->texture);
  SDL_DestroyRenderer(graphics->renderer);
  SDL_DestroyWindow(graphics->window); 
  SDL_Quit();
  return 0;
}

void integrate(Player * player, double dt) {
  player->vx += player->ax * dt;
  player->x += player->vx * dt;

  player->vy += player->ay * dt;
  player->y += player->vy * dt;
}

void render(Player * player, Graphics * graphics) {
  SDL_Rect dstrect = { player->x, player->y, 64, 64 };
  SDL_RenderClear(graphics->renderer);
  SDL_RenderCopy(graphics->renderer, graphics->texture, NULL, &dstrect);
  SDL_RenderPresent(graphics->renderer);
}

int main(int argc, char ** argv)
{
  SDL_Init(SDL_INIT_VIDEO);

  bool quit = false;
  Graphics graphics = initGraphics();

  Player curPlayer = initPlayer();
  Player prevPlayer;

  Time time = initTime();

  SDL_SetRenderDrawColor(graphics.renderer, 255, 255, 255, 255);

  while (!quit)
  {
    time_t newTime = currentTime();
    double frameTime = fabs(difftime(newTime, time.t));
    if ( frameTime > 0.25 ) {
      frameTime = 0.25;
    }
    time.t = newTime;

    time.acc += frameTime;

    while ( time.acc >= time.dt )
    {
      prevPlayer = curPlayer;
      integrate(&curPlayer, time.dt);
      time.acc -= time.dt;
    }

    const double alpha = time.acc / time.dt;

    Player interpolatedPlayer = {
      .x = curPlayer.x * alpha + prevPlayer.x * ( 1.0 - alpha),
      .y = curPlayer.y * alpha + prevPlayer.y * ( 1.0 - alpha),
    };

    render(&interpolatedPlayer, &graphics);
    handleEvents(&curPlayer, &quit);
  }
  return cleanUpAndExit(&graphics);
}
