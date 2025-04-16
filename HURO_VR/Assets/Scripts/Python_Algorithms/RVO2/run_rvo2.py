#!/usr/bin/env python
import rvo2

def run_rvo2(scene_data):
      sim = rvo2.PyRVOSimulator(
            1/60., # timeStep
            1.5, # neighborDist
            5, # maxNeighbors
            1.5, # timeHorizon
            2, # timeHorizonObst
            0.4, # radius
            2) # maxSpeed

      # Pass either just the position (the other parameters then use
      # the default values passed to the PyRVOSimulator constructor),
      # or pass all available parameters.
      agent_nos = []
      for robot in scene_data.robots:
            num = sim.addAgent((robot.position.x, robot.position.z))
            sim.setAgentPrefVelocity(num, (0, 0))
            agent_nos.append(num)

      # Obstacles are also supported.
      for obstacle in scene_data.obstacles:
            # sim.addObstacle([<vertex0>, <vertex1>, <vertex2>, ...])
            sim.addObstacle([(obstacle.vertex.x, obstacle.vertex.z) for obstacle in obstacle.vertices])

      sim.processObstacles()

      # Make agent 0 much less collaborative (nominally does 0.5 of the avoidance)
      sim.setAgentCollabCoeff(agent_nos[0], 0.1)

      sim.doStep()

      newVelocities = []
      for agent_no in agent_nos:
            newVelocities.append([sim.getAgentVelocity(agent_no)[0], sim.getAgentVelocity(agent_no)[1]])

      return newVelocities
      

