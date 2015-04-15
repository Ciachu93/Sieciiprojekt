section .text
global _start
_start:


mov eax,3
mov ebx,0
mov rcx,a
mov edx,[ad]
int 80h

mov eax,3
mov ebx,0
mov rcx,b
mov edx,[bd]
int 80h

mov eax,[a]
sub eax,48
mov ebx,[b]
sub ebx,48
imul eax,ebx
add eax,48
mov [wynik],eax

xor eax,eax
xor ebx,ebx

mov eax,[wynik]
sub eax,48
cmp al,10

jb _koniec

mov eax,4
mov ebx,1
mov rcx,wieksze
mov edx,[wiekszed]
int 80h

mov eax,[wynik]

mov edx,0
mov ecx,10
mov eax,[wynik]
sub eax,48
div ecx

add eax,48
add edx,48
mov [reszta],edx
mov [temp],eax

xor edx,edx

mov eax,4
mov ebx,1
mov ecx,temp
mov edx,1
int 80h
mov eax,4
mov ebx,1
mov ecx,reszta
mov edx,1
int 80h
jmp _wyjscie

_koniec:

mov eax,4
mov ebx,1
mov rcx,wynik
mov edx,1
int 80h    

_wyjscie:

mov eax,1
mov ebx,0
int 80h

section .data


wieksze db "Wieksze lub rowne 10",0ah
wiekszed dd $-wieksze

reszta dd 0
temp dd 0
a dd 0
ad dd $-a
b dd 0
bd dd $-b
wynik dd 0
wynikd dd $-wynik