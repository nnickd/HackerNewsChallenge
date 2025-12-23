import { Directive, HostListener, Input, EventEmitter, Output } from '@angular/core';

@Directive({
  selector: '[appKeyboardShortcuts]',
  standalone: true
})
export class KeyboardShortcutsDirective {
  @Input() shortcutsDisabled = false;

  @Output() focusTarget = new EventEmitter<void>();
  @Output() prevPage = new EventEmitter<void>();
  @Output() nextPage = new EventEmitter<void>();
  @Output() nextItem = new EventEmitter<void>();
  @Output() prevItem = new EventEmitter<void>();
  @Output() escape = new EventEmitter<void>();
  @Output() control = new EventEmitter<void>();
  


  constructor() { }

  @HostListener('window:keydown', ['$event'])
  onKeyDown(evt: KeyboardEvent): void {
    if (this.shortcutsDisabled) return;

    const target = evt.target as HTMLElement | null;
    const tag = target?.tagName?.toLowerCase();

    const isTypingContext = tag == 'input' || 
                            tag == 'textarea' || 
                            tag == 'select' || 
                            (target as any)?.isContentEditable;

    if (!isTypingContext && evt.key == '/') {
      evt.preventDefault();
      this.focusTarget.emit();
      // this.focusTarget?.focus();
      // this.focusTarget?.select();
      return;
    }

    if (evt.key == 'Escape') {
      this.escape.emit();
      return;
    }

    if (!isTypingContext && evt.key == 'ArrowLeft') {
      evt.preventDefault();
      this.prevPage.emit();
      return;
    }

    if (!isTypingContext && evt.key == 'ArrowRight') {
      evt.preventDefault();
      this.nextPage.emit();
      return;
    }

    if (evt.key == 'Control') {
      evt.preventDefault();
      this.control.emit();
      return;
    }

    if (evt.key == 'ArrowUp') {
      evt.preventDefault();
      this.prevItem.emit();
      return;
    }

    if (evt.key == 'ArrowDown') {
      evt.preventDefault();
      this.nextItem.emit();
      return;
    }
  }
}
