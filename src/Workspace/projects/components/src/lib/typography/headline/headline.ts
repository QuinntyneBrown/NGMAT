import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

type HeadlineLevel = 1 | 2 | 3 | 4 | 5 | 6;

@Component({
  selector: 'g-headline',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './headline.html',
  styleUrls: ['./headline.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.g-headline]': 'true',
    '[class.g-headline--1]': 'level === 1',
    '[class.g-headline--2]': 'level === 2',
    '[class.g-headline--3]': 'level === 3',
    '[class.g-headline--4]': 'level === 4',
    '[class.g-headline--5]': 'level === 5',
    '[class.g-headline--6]': 'level === 6',
  },
})
export class Headline {
  @Input() level: HeadlineLevel = 1;
}
