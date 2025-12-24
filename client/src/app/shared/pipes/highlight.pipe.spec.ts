import { HighlightPipe } from './highlight.pipe';

describe('HighlightPipe', () => {
  let pipe: HighlightPipe;

  beforeEach(() => {
    pipe = new HighlightPipe();
  });

  it('should return empty non-matching part for empty value', () => {
    const result = pipe.transform('', 'test');
    expect(result).toEqual([{ text: '', match: false }]);
  });

  it('should return all parts as non-matching when query is empty', () => {
    const result = pipe.transform('Hello world', '   ');

    expect(result).toEqual([
      { text: 'Hello', match: false },
      { text: ' ', match: false },
      { text: 'world', match: false },
    ]);
  });

  it('should highlight exact matches for short query (<= 3 chars)', () => {
    const result = pipe.transform('foo bar baz', 'bar');

    expect(result).toEqual([
      { text: 'foo', match: false },
      { text: ' ', match: false },
      { text: 'bar', match: true },
      { text: ' ', match: false },
      { text: 'baz', match: false },
    ]);
  });

  it('should highlight partial matches for long query (> 3 chars)', () => {
    const result = pipe.transform('Angular Highlight Pipe', 'high');

    expect(result).toEqual([
      { text: 'Angular', match: false },
      { text: ' ', match: false },
      { text: 'Highlight', match: true },
      { text: ' ', match: false },
      { text: 'Pipe', match: false },
    ]);
  });

  it('should ignore punctuation when matching', () => {
    const result = pipe.transform('Hello, world!', 'world');

    expect(result).toEqual([
      { text: 'Hello,', match: false },
      { text: ' ', match: false },
      { text: 'world!', match: true },
    ]);
  });

  it('should be case-insensitive when matching', () => {
    const result = pipe.transform('Angular PIPE', 'pipe');

    expect(result).toEqual([
      { text: 'Angular', match: false },
      { text: ' ', match: false },
      { text: 'PIPE', match: true },
    ]);
  });
});
