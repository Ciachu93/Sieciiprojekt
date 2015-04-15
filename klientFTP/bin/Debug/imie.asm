section .text
global _start
_start:


mov eax,3
mov ebx,0
mov ecx,tekst
mov edx,[dlg]
int 80h

mov ecx,[dlg]
mov eax,[licznik]
mov edi,tekst
mov sil,0ah

_petla:

    push rcx     
    
    cmp sil,[edi]
    je _wypisz
    
    inc edi
    inc eax
    
    pop rcx
    
    loop _petla

_wypisz:    




mov [licznik],eax

mov eax,4
mov ebx,1
mov ecx,tekst
mov edx,[licznik]
int 80h

mov eax,4
mov ebx,1
mov ecx,pytajnik
mov edx,1
int 80h

mov eax,1
mov ebx,0
int 80h

section .data


pytajnik db '?'	
tekst TIMES 1000 db 0
    db "?"
dlg dd $-tekst
licznik dd 0
dlgl dd $-licznik