import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

type TextVariant = 'body-1' | 'body-2' | 'caption' | 'overline';
type TextColor = 'primary' | 'secondary' | 'disabled' | 'hint' | 'inherit';

@Component({
  selector: 'g-text',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './text.html',
  styleUrls: ['./text.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.g-text]': 'true',
    '[class.g-text--body-1]': 'variant === "body-1"',
    '[class.g-text--body-2]': 'variant === "body-2"',
    '[class.g-text--caption]': 'variant === "caption"',
    '[class.g-text--overline]': 'variant === "overline"',
    '[class.g-text--primary]': 'color === "primary"',
    '[class.g-text--secondary]': 'color === "secondary"',
    '[class.g-text--disabled]': 'color === "disabled"',
    '[class.g-text--hint]': 'color === "hint"',
  },
})
export class Text {
  @Input() variant: TextVariant = 'body-1';
  @Input() color: TextColor = 'inherit';
}
