\ Most of this stuff is based on Timothy Trussell's gforth sdl opengl lessons:
\ ftp://ftp.taygeta.com/pub/Forth/Archive/tutorials/gforth-sdl-opengl/

c-library sdl2_lib
s" SDL2" add-lib

\c #include <SDL2/SDL.h>

c-function sdl-init             SDL_Init            u -- n
c-function sdl-quit		          SDL_Quit		        -- void

c-function sdl-create-window    SDL_CreateWindow    s n n n n u -- a
c-function sdl-destroy-window   SDL_DestroyWindow   a -- void
c-function sdl-set-window-title SDL_SetWindowTitle  a s -- void

c-function sdl-load-bmp-rw      SDL_LoadBMP_RW      a n -- a
c-function sdl-rw-from-file     SDL_RWFromFile      s s -- a
c-function sdl-free-surface     SDL_FreeSurface     a -- void

c-function sdl-create-renderer  SDL_CreateRenderer  a n u -- a
c-function sdl-destroy-renderer SDL_DestroyRenderer a -- void
c-function sdl-create-texture-from-surface SDL_CreateTextureFromSurface   a a -- a
c-function sdl-render-clear     SDL_RenderClear     a -- n
c-function sdl-render-copy      SDL_RenderCopy      a a a a -- n
c-function sdl-render-copy-f    SDL_RenderCopyF     a a a a -- n
c-function sdl-render-present   SDL_RenderPresent   a -- void
c-function sdl-destroy-texture  SDL_DestroyTexture  a -- void
c-function sdl-set-render-draw-color SDL_SetRenderDrawColor   a u u u u -- n

c-function sdl-geterror         SDL_GetError      -- s

c-function sdl-poll-event       SDL_PollEvent     a -- n
c-function sdl-wait-event       SDL_WaitEvent     a -- n

c-function sdl-get-keyboard-state SDL_GetKeyboardState a -- a

end-c-library

: sdl-load-bmp ( addr u -- bmp_addr )
  s" rb" sdl-rw-from-file 1 sdl-load-bmp-rw ;

1 1 2constant byte%
4 4 2constant int%

256 constant SDL_QUIT

$00000020	constant SDL_INIT_VIDEO
$1FFF0000 constant SDL_WINDOWPOS_UNDEFINED

4 constant SDL_SCANCODE_A
7 constant SDL_SCANCODE_D
22 constant SDL_SCANCODE_S
26 constant SDL_SCANCODE_W

struct
  int% field x
  int% field y
  int% field w
  int% field h
end-struct sdl-rect%

struct
  float% field fx
  float% field fy
  float% field fw
  float% field fh
end-struct sdl-frect%

struct
  int% field sdl-event-type
  1 0   field sdl-event-payload  \ 1st field in each STRUCT maps here
end-struct sdl-event-type%

sdl-event-type%
  1 0 field sdl-quit-event-type
end-struct sdl-quit-event%

\ The SDL_Event structure is arranged as a UNION.
\ This means that each of the structures is accessed at the same
\ event address. Therefore, we only have to allocate memory for the
\ largest struct size we will need to access.
\
\ All of the data can now be referenced by using the address of the
\ event struct in gforth memory, followed by the field name of the
\ specific event being processed.
\
\ All of the -type fields in the structs are mapped to the address of
\ the sdl-event-type field in the sdl-event% struct.

sdl-event-type%
  1 31 field sdl-event-data      \ actually 8 bytes, but I padded it
end-struct sdl-event%
