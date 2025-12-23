import { Pipe, PipeTransform } from '@angular/core';

export type HighlightPart = { text: string; match: boolean };

@Pipe({
  name: 'highlight'
})
export class HighlightPipe implements PipeTransform {

  private readonly trimRegex = /^[^\w]+|[^\w]+$/g;

  transform(value: string, match: string | null | undefined): HighlightPart[] {
    if (!value) {
      return [{ text: '', match: false }];
    }
    let parts: HighlightPart[] = [];

    let valueParts = value.split(/(\s+)/);
    if (!match || match.trim() == '') {
      for (let v of valueParts) {
        parts.push({ text: v, match: false });
      }
      return parts
    }

    let matchParts = match
                    .split(/\s+/)
                    .map(q => q.replace(this.trimRegex, '').toLowerCase())
                    .filter(q => q.length > 0);

    for (let v of valueParts) {
      let part = {
        text: v,
        match: false
      }

      let cleanedValue = v.replace(this.trimRegex, '').toLowerCase();

      if (!/^\s+$/.test(v) && cleanedValue) {
        for (let m of matchParts) {
          if (m.length <= 3) {
            if (cleanedValue == m) {
              part.match = true;
            }
          } else if (cleanedValue.includes(m)) {
              part.match = true;
          }
          if (part.match) {
            break;
          }
        }
      }

      parts.push(part);
    }

    return parts;
  }
}