import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

type AvatarSize = 'sm' | 'md' | 'lg' | 'xl';

@Component({
  selector: 'g-avatar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './avatar.html',
  styleUrls: ['./avatar.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.g-avatar]': 'true',
    '[class.g-avatar--sm]': 'size === "sm"',
    '[class.g-avatar--md]': 'size === "md"',
    '[class.g-avatar--lg]': 'size === "lg"',
    '[class.g-avatar--xl]': 'size === "xl"',
  },
})
export class Avatar {
  @Input() src?: string;
  @Input() initials?: string;
  @Input() size: AvatarSize = 'md';
}
