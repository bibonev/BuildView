#include <iostream>
#include <pcl/io/pcd_io.h>
#include <pcl/point_types.h>
#include <pcl/filters/statistical_outlier_removal.h>
#include <pcl/point_types.h>
#include <pcl/io/pcd_io.h>
#include <pcl/io/ply_io.h>
#include <pcl/io/obj_io.h>
#include <pcl/kdtree/kdtree_flann.h>
#include <pcl/features/normal_3d.h>
#include <pcl/surface/gp3.h>
#include <pcl/surface/mls.h>

void StatisticalOutlierRemoval (int meanK, double treshold, char* name, char* outname)
{
  pcl::PointCloud<pcl::PointXYZRGBA>::Ptr cloud (new pcl::PointCloud<pcl::PointXYZRGBA>);
  pcl::PointCloud<pcl::PointXYZRGBA>::Ptr cloud_filtered (new pcl::PointCloud<pcl::PointXYZRGBA>);

  // Fill in the cloud data
  pcl::PCDReader reader;
  // Replace the path below with the path where you saved your file
  reader.read<pcl::PointXYZRGBA> (name, *cloud);

  std::cerr << "Cloud before filtering: " << std::endl;
  std::cerr << *cloud << std::endl;

  // Create the filtering object
  pcl::StatisticalOutlierRemoval<pcl::PointXYZRGBA> sor;
  sor.setInputCloud (cloud);
  sor.setMeanK (meanK);
  sor.setStddevMulThresh (treshold);
  sor.filter (*cloud_filtered);

  std::cerr << "Cloud after filtering: " << std::endl;
  std::cerr << *cloud_filtered << std::endl;

  pcl::PCDWriter writer;
  writer.write<pcl::PointXYZRGBA> (outname, *cloud_filtered, true);
  pcl::io::savePLYFile("test.ply", *cloud, false);
}

void Smoothing(char* name, char* outname)
{
  // Load input file into a PointCloud<T> with an appropriate type
  pcl::PointCloud<pcl::PointXYZRGBA>::Ptr cloud (new pcl::PointCloud<pcl::PointXYZRGBA> ());

  // Fill in the cloud data
  pcl::PCDReader reader;
  // Replace the path below with the path where you saved your file
  reader.read<pcl::PointXYZRGBA> (name, *cloud);

  // Create a KD-Tree
  pcl::search::KdTree<pcl::PointXYZRGBA>::Ptr tree (new pcl::search::KdTree<pcl::PointXYZRGBA>);

  // Output has the PointNormal type in order to store the normals calculated by MLS
  //pcl::PointCloud<pcl::PointXYZRGBA> mls_points;
  pcl::PointCloud<pcl::PointXYZRGBA>::Ptr mls_points (new pcl::PointCloud<pcl::PointXYZRGBA>);

  // Init object (second point type is for the normals, even if unused)
  pcl::MovingLeastSquares<pcl::PointXYZRGBA, pcl::PointXYZRGBA> mls;
 
  mls.setComputeNormals (true);

  // Set parameters
  mls.setInputCloud (cloud);
  mls.setPolynomialFit (true);
  mls.setSearchMethod (tree);
  mls.setSearchRadius (100.03);

  // Reconstruct
  mls.process(*mls_points);

  // Save output
  pcl::PCDWriter writer;
  writer.write<pcl::PointXYZRGBA> (outname, *mls_points, true);
}

void CreateMesh(char* name, char* outname)
{
  // Load input file into a PointCloud<T> with an appropriate type
  pcl::PointCloud<pcl::PointXYZRGB>::Ptr cloud (new pcl::PointCloud<pcl::PointXYZRGB>);
  // Fill in the cloud data
  //pcl::PCDReader reader;
  // Replace the path below with the path where you saved your file
  //reader.read<pcl::PointXYZRGB> ("PCL.pcd", *cloud
  sensor_msgs::PointCloud2 cloud_blob;
  pcl::io::loadPCDFile (name, cloud_blob);
  pcl::fromROSMsg (cloud_blob, *cloud);
  //* the data should be available in cloud
  std::cerr << "Cloud before filtering: " << std::endl;
  std::cerr << *cloud << std::endl;

  // Normal estimation*
  pcl::NormalEstimation<pcl::PointXYZRGB, pcl::Normal> n;
  pcl::PointCloud<pcl::Normal>::Ptr normals (new pcl::PointCloud<pcl::Normal>);
  pcl::search::KdTree<pcl::PointXYZRGB>::Ptr tree (new pcl::search::KdTree<pcl::PointXYZRGB>);
  tree->setInputCloud (cloud);
  n.setInputCloud (cloud);
  n.setSearchMethod (tree);
  n.setKSearch (20);
  n.compute (*normals);
  //* normals should not contain the point normals + surface curvatures

  // Concatenate the XYZ and normal fields*
  pcl::PointCloud<pcl::PointXYZRGBNormal>::Ptr cloud_with_normals (new pcl::PointCloud<pcl::PointXYZRGBNormal>);
  pcl::concatenateFields (*cloud, *normals, *cloud_with_normals);
  //* cloud_with_normals = cloud + normals

  // Create search tree*
  pcl::search::KdTree<pcl::PointXYZRGBNormal>::Ptr tree2 (new pcl::search::KdTree<pcl::PointXYZRGBNormal>);
  tree2->setInputCloud (cloud_with_normals);  

  // Initialize objects
  pcl::GreedyProjectionTriangulation<pcl::PointXYZRGBNormal> gp3;
  pcl::PolygonMesh triangles;

  // Set the maximum distance between connected points (maximum edge length)
  gp3.setSearchRadius (0.50);

  // Set typical values for the parameters
  gp3.setMu (5.00);
  gp3.setMaximumNearestNeighbors (50);
  gp3.setMaximumSurfaceAngle(M_PI/4); // 45 degrees
  gp3.setMinimumAngle(M_PI/180); // 1 degrees
  gp3.setMaximumAngle(2*M_PI/2); // 180 degrees
  gp3.setNormalConsistency(false);

  // Get result
  gp3.setInputCloud (cloud_with_normals);
  gp3.setSearchMethod (tree2);
  gp3.reconstruct (triangles);

  ofstream myFile (outname, ios::out | ios::binary);

  for (size_t i = 0; i < triangles.polygons.size(); ++i)
  {
	uint32_t v;
	v = triangles.polygons[i].vertices[0];
    myFile.write ((char*)&v, sizeof (uint32_t));
	pcl::PointXYZRGB test;
	
	v = triangles.polygons[i].vertices[1];
    myFile.write ((char*)&v, sizeof (uint32_t));

	v = triangles.polygons[i].vertices[2];
    myFile.write ((char*)&v, sizeof (uint32_t));
  }
  myFile.close();

  //pcl::io::saveOBJFile("model.obj", triangles);
}

int main (int argc, char** argv)
{
  if (argc > 1)
  {
	  if (!strcmp(argv[1], "StatisticalOutlierRemoval"))
	  {
		  StatisticalOutlierRemoval(50, 1.0, argv[2], argv[3]);
		  return 0;
	  }
	  else if (!strcmp(argv[1], "CreateMesh"))
	  {
		  CreateMesh(argv[2], argv[3]);
		  return 0;
	  }
	  else if (!strcmp(argv[1], "Smoothing"))
	  {
		  CreateMesh(argv[2], argv[3]);
		  return 0;
	  }
  }
  
  cout<<"Use Help :)"<<endl;

  return 0;
}

