extends Node2D

#get the master node
@onready var Game = get_parent()

#get all the necessary tilemaps for this area
@onready var DummyObject = get_node("DummyObject")
@onready var Objects1 = get_node("Objects1")
@onready var Objects2 = get_node("Objects2")
@onready var Objects3 = get_node("Objects3")

#declare the size of this area and the tile sizes of this area
const grid_size = Vector2(75, 100)
var tile_size #32 pixels x 32 pixels
var half_tile_size
var grid = []

func _get_tile(tile_map, x, y):
	return tile_map.get_cell_source_id(0, Vector2i(int(x), int(y)))

func _grid_direction(direction):
	return Vector2i(int(direction.x), int(direction.y))

func _ready():
	tile_size = Game.tile_size #get the tile size from Game
	half_tile_size = Game.half_tile_size

	for x in range(grid_size.x):
		grid.append([])
		for y in range(grid_size.y):
			if _get_tile(Objects1, x, y) != -1 or _get_tile(Objects2, x, y) != -1 or _get_tile(Objects3, x, y) != -1: #add all objects to the grid
				grid[x].append(1)
			else:
				grid[x].append(null)

#this function tells the player if they are about to be teleported to a new area
func teleport(position):
	if DummyObject.local_to_map(position) == Vector2i(37, 93):
		return true
	return false

#checks if this cell is vacant
func is_cell_vacant(pos, direction):
	var grid_pos = DummyObject.local_to_map(pos) + _grid_direction(direction)
	
#	if grid_pos.x < grid_size.x and grid_pos.x >= 0:
#		if grid_pos.y < grid_size.y and grid_pos.y >= 0:
	if grid[grid_pos.x][grid_pos.y] != 1:
			return true
	
	return false

#updates the grid position for the player
func update_child_pos(child_node):
	var grid_pos = DummyObject.local_to_map(child_node.position)
	grid[grid_pos.x][grid_pos.y] = null
	
	var new_grid_pos = grid_pos + _grid_direction(child_node.direction)
	
	var target_pos = DummyObject.map_to_local(new_grid_pos) + half_tile_size
	return target_pos

#returns true if the player can open the shop menu, false otherwise
func can_shop(position):
	if DummyObject.local_to_map(position) == Vector2i(27, 43):
		return true
	return false
