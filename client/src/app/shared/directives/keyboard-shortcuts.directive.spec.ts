import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { KeyboardShortcutsDirective } from './keyboard-shortcuts.directive';

@Component({
  template: `
    <input />
    <div
      appKeyboardShortcuts
      (focusTarget)="onFocus()"
      (prevPage)="onPrevPage()"
      (nextPage)="onNextPage()"
      (prevItem)="onPrevItem()"
      (nextItem)="onNextItem()"
      (escape)="onEscape()"
    ></div>
  `,
  standalone: true,
  imports: [KeyboardShortcutsDirective],
})
class TestHostComponent {
  focus = false;
  prevPage = false;
  nextPage = false;
  prevItem = false;
  nextItem = false;
  escape = false;

  onFocus() { this.focus = true; }
  onPrevPage() { this.prevPage = true; }
  onNextPage() { this.nextPage = true; }
  onPrevItem() { this.prevItem = true; }
  onNextItem() { this.nextItem = true; }
  onEscape() { this.escape = true; }
}

describe('KeyboardShortcutsDirective', () => {
  let host: TestHostComponent;

  function dispatch(key: string, target: EventTarget = document.body) {
    window.dispatchEvent(
      new KeyboardEvent('keydown', { key, bubbles: true })
    );
  }

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [TestHostComponent],
    });

    const fixture = TestBed.createComponent(TestHostComponent);
    host = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should emit focusTarget on "/" outside typing context', () => {
    dispatch('/');
    expect(host.focus).toBeTrue();
  });

  it('should emit escape on Escape', () => {
    dispatch('Escape');
    expect(host.escape).toBeTrue();
  });

  it('should emit page navigation keys outside typing context', () => {
    dispatch('ArrowLeft');
    dispatch('ArrowRight');

    expect(host.prevPage).toBeTrue();
    expect(host.nextPage).toBeTrue();
  });

  it('should emit item navigation keys outside typing context', () => {
    dispatch('ArrowUp');
    dispatch('ArrowDown');

    expect(host.prevItem).toBeTrue();
    expect(host.nextItem).toBeTrue();
  });

  it('should NOT emit navigation events when typing in input (except ArrowDown)', () => {
    const input = document.querySelector('input')!;
    input.focus();

    input.dispatchEvent(new KeyboardEvent('keydown', { key: 'ArrowUp', bubbles: true }));
    input.dispatchEvent(new KeyboardEvent('keydown', { key: 'ArrowLeft', bubbles: true }));
    input.dispatchEvent(new KeyboardEvent('keydown', { key: 'ArrowRight', bubbles: true }));

    expect(host.prevItem).toBeFalse();
    expect(host.nextItem).toBeFalse();
    expect(host.prevPage).toBeFalse();
    expect(host.nextPage).toBeFalse();
  });


  it('should still emit escape when typing', () => {
    const input = document.querySelector('input')!;
    input.focus();

    input.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));
    expect(host.escape).toBeTrue();
  });
});