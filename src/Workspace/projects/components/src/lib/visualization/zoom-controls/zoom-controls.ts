import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'g-zoom-controls',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatTooltipModule],
  templateUrl: './zoom-controls.html',
  styleUrls: ['./zoom-controls.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ZoomControls {
  @Input() level = 100;
  @Input() min = 25;
  @Input() max = 400;

  @Output() zoomIn = new EventEmitter<void>();
  @Output() zoomOut = new EventEmitter<void>();
  @Output() reset = new EventEmitter<void>();

  get canZoomIn(): boolean {
    return this.level < this.max;
  }

  get canZoomOut(): boolean {
    return this.level > this.min;
  }

  onZoomIn(): void {
    if (this.canZoomIn) {
      this.zoomIn.emit();
    }
  }

  onZoomOut(): void {
    if (this.canZoomOut) {
      this.zoomOut.emit();
    }
  }

  onReset(): void {
    this.reset.emit();
  }
}
