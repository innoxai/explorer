import { Vector3 } from './Vector3'

/**
 * Defines the 3 main axes
 * @public
 */
export class Axis {
  /** X axis */
  public static X: Vector3 = new Vector3(1.0, 0.0, 0.0)
  /** Y axis */
  public static Y: Vector3 = new Vector3(0.0, 1.0, 0.0)
  /** Z axis */
  public static Z: Vector3 = new Vector3(0.0, 0.0, 1.0)
}
