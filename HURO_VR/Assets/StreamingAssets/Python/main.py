import json
from RVO import RVO_update, compute_V_des
from StringToObject import StringToObject
import argparse

def main():
    parser = argparse.ArgumentParser(description="Calculate square velocity.")
    parser.add_argument("json_str", type=str, help="object data")
    args = parser.parse_args()
    scene_data = StringToObject(args.json_str)
    #------------------------------
    #define workspace model
    ws_model = dict()

    #robot radius
    ws_model['robot_radius'] = scene_data.robot_radius

    #circular obstacles, format [x,y,rad]
    ws_model['circular_obstacles'] = [[obstacle.position.x, obstacle.position.z, obstacle.radius] for obstacle in scene_data.obstacles]
    # with obstacles
    # ws_model['circular_obstacles'] = [[-0.3, 2.5, 0.3], [1.5, 2.5, 0.3], [3.3, 2.5, 0.3], [5.1, 2.5, 0.3]]

    #rectangular boundary, format [x,y,width/2,heigth/2]
    ws_model['boundary'] = [scene_data.boundary.position.x, 
                            scene_data.boundary.position.z, 
                            scene_data.boundary.width/2, 
                            scene_data.boundary.length/2
                        ]
    
    # # compute desired vel to goal
    robot_positions = [[robot.position.x, robot.position.z] for robot in scene_data.robots]
    goal_positions = [[robot.goal.x, robot.goal.z] for robot in scene_data.robots]
    maxVelocities = [robot.max_velocity for robot in scene_data.robots]

    # Max velocities must be at least 2
    if (len(maxVelocities) == 1):
        maxVelocities.append(scene_data.robots[0].max_velocity)
    currVelocity = [[robot.curr_velocity.x, robot.curr_velocity.z] for robot in scene_data.robots]
    
    V_des = compute_V_des(robot_positions, goal_positions, maxVelocities)
    
    # compute the optimal vel to avoid collision
    new_velocities = RVO_update(robot_positions, V_des, currVelocity, ws_model)
    print(json.dumps(new_velocities))
    return json.dumps(new_velocities)

if __name__ == "__main__":
    main()